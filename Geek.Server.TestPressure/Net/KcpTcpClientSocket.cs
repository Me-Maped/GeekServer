﻿using Geek.Server.Core.Net.Kcp;
using Geek.Server.Core.Utils;
using MessagePack;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace Geek.Server.TestPressure.Net
{
    public class KcpTcpClientSocket : AKcpSocket
    {
        public delegate void ReceiveFunc(TempNetPackage package);
        //ConnectionContext context;
        private TcpClient socket;
        Pipe dataPipe;

        public KcpTcpClientSocket(int serverId)
        {
            ServerId = serverId;
        }

#if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string getIPv6(string mHost, string mPort);
#endif
        private static string GetIPv6(string mHost, string mPort)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
		    string mIPv6 = getIPv6(mHost, mPort);
		    return mIPv6;
#else
            return mHost + "&&ipv4";
#endif
        }

        TcpClient createSockect(ref string ip, int port)
        {
            AddressFamily ipType = AddressFamily.InterNetwork;
            try
            {
                string ipv6 = GetIPv6(ip, port.ToString());
                if (!string.IsNullOrEmpty(ipv6))
                {
                    string[] tmp = System.Text.RegularExpressions.Regex.Split(ipv6, "&&");
                    if (tmp != null && tmp.Length >= 2)
                    {
                        string type = tmp[1];
                        if (type == "ipv6")
                        {
                            ip = tmp[0];
                            ipType = AddressFamily.InterNetworkV6;
                        }
                    }
                }
                return new TcpClient(ipType);
            }
            catch (Exception e)
            {
                LOGGER.Error(e.Message + "\n" + e.StackTrace);
                return null;
            }
        }

        public override async Task<ConnectResult> Connect(string ip, int port, long netId = 0)
        {
            LOGGER.Info($"连接:{ip} {port}");
            isConnecting = true;
            NetId = netId;
            //context = await new SocketConnection(AddressFamily.InterNetwork, ip, port).StartAsync();
            socket = createSockect(ref ip, port);
            if (socket == null)
                return new(false, true, false);

            try
            {
                var task = socket.ConnectAsync(ip, port);
                var tokenSource = new CancellationTokenSource();
                var completeTask = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(5), tokenSource.Token));
                if (completeTask != task)
                {
                    Close();
                    return new(false, true, false);
                }
                else
                {
                    tokenSource.Cancel();
                    await task;
                }
            }
            catch (Exception)
            {
                Close();
                return new(false, true, false);
            }

            dataPipe = new Pipe();
            _ = readSocketData();

            Send(new TempNetPackage(NetPackageFlag.SYN, NetId, ServerId));

            ConnectResult retResult = new()
            {
                allowReconnect = true
            };

            //等待连接消息
            if (!cancelSrc.IsCancellationRequested)
            {
                var cancelToken = cancelSrc.Token;
                while (!cancelSrc.IsCancellationRequested)
                {
                    try
                    {
                        var result = await dataPipe.Reader.ReadAsync(cancelToken);
                        var buf = result.Buffer;
                        if (TryParseNetPack(ref buf, (pack) =>
                        {
                            NetId = pack.netId;
                            if (pack.flag == NetPackageFlag.ACK)
                            {
                                LOGGER.Info($"连接成功..");
                                retResult.isSuccess = true;
                            }
                            if (pack.flag == NetPackageFlag.NO_GATE_CONNECT)
                            {
                            }
                            if (pack.flag == NetPackageFlag.NO_INNER_SERVER)
                            {
                                retResult.allowReconnect = false;
                                retResult.resetNetId = true;
                            }
                            if (pack.flag == NetPackageFlag.CLOSE)
                            {
                                retResult.resetNetId = true;
                            }
                        }))
                        {
                            dataPipe.Reader.AdvanceTo(buf.Start, buf.End);
                            break;
                        }
                        else
                        {
                            dataPipe.Reader.AdvanceTo(buf.Start, buf.End);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            isConnecting = false;
            return retResult;
        }

        async Task readSocketData()
        {
            byte[] readBuffer = new byte[1024 * 20];
            var dataPipeWriter = dataPipe.Writer;
            var cancelToken = cancelSrc.Token;
            while (!cancelSrc.IsCancellationRequested && !IsClose())
            {
                try
                {
                    var length = await socket.GetStream().ReadAsync(readBuffer, 0, readBuffer.Length, cancelToken);
                    if (length > 0)
                    {
                        //Debug.Log($"收到网络包：{length}");
                        dataPipeWriter.Write(readBuffer.AsSpan()[..length]);
                        var flushTask = dataPipeWriter.FlushAsync();
                        if (!flushTask.IsCompleted)
                        {
                            await flushTask.ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch
                {
                    break;
                }
            }
            Close();
        }

        async Task ReadPackAsync(ReceiveFunc onPack)
        {
            var readCancelToken = cancelSrc.Token;
            var reader = dataPipe.Reader;
            while (!cancelSrc.IsCancellationRequested)
            {
                try
                {
                    var result = await reader.ReadAsync(readCancelToken);
                    //LOGGER.Info($"read tcp len:{result.Buffer.Length}");
                    var buf = result.Buffer;
                    if (buf.Length > 0)
                    {
                        while (TryParseNetPack(ref buf, onPack))
                        {

                        }
                        reader.AdvanceTo(buf.Start, buf.End);
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        bool TryParseNetPack(ref ReadOnlySequence<byte> input, ReceiveFunc onPack)
        {
            var netReader = dataPipe.Reader;
            var reader = new SequenceReader<byte>(input);

            if (!reader.TryReadBigEndian(out int msgLen))
            {
                return false;
            }

            if (msgLen < TempNetPackage.headLen)
            {
                throw new Exception($"从客户端接收的包大小异常,{msgLen} {reader.Remaining}:至少大于8个字节");
            }
            else if (msgLen > 1500)
            {
                throw new Exception("从客户端接收的包大小超过限制：" + msgLen + "字节，最大值1500字节");
            }

            if (reader.Remaining < msgLen)
            {
                return false;
            }

            reader.TryRead(out byte flag);
            reader.TryReadBigEndian(out long netId);
            reader.TryReadBigEndian(out int serverId);
            var dataLen = msgLen - TempNetPackage.headLen;

            var payload = input.Slice(reader.Position, dataLen);
            Span<byte> data = stackalloc byte[dataLen];
            payload.CopyTo(data);
            onPack(new TempNetPackage(flag, netId, serverId, data));
            input = input.Slice(msgLen + 4);
            return true;
        }

        public override bool IsClose()
        {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return true;
        }
#endif
            if (isConnecting)
                return false;
            if (socket == null)
            {
                return true;
            }
            if (!socket.Connected)
                return true;
            try
            {
                if (socket.Client.Poll(1000, SelectMode.SelectRead) && socket.Client.Available == 0)
                {
                    return true;
                }
            }
            catch
            {
                return true;
            }

            return false;
        }

        public override void Close()
        {
            lock (this)
            {
                try
                {
                    if (socket != null)
                    {
                        base.Close();
                        socket.Close();
                        socket.Dispose();
                    }
                }
                catch { }
                finally
                {
                    dataPipe = null;
                    socket = null;
                }
            }
        }

        public override void Send(TempNetPackage package)
        {
            Span<byte> target = stackalloc byte[package.Length + 4];
            int offset = 0;
            target.Write(package.Length, ref offset);
            target.Write(package, ref offset);
            //netWriter.Write(target);
            ////Debug.Log($"写tcp pack:{package.ToString()}");
            //netWriter.FlushAsync();

            if (IsClose())
                return;
            socket.GetStream().Write(target);
        }

        public override async Task StartRecv(OnReceiveNetPackFunc onRecv, Action onGateClose, Action onServerClose)
        {
            _ = StartGateHeartAsync();
            await ReadPackAsync((package) =>
            {
#if UNITY_EDITOR
            //Debuger.Log($"收到包...{package.ToString()}");
#endif
                if (package.netId != NetId)
                    return;
                switch (package.flag)
                {
                    case NetPackageFlag.NO_GATE_CONNECT:
                        Close();
                        onGateClose?.Invoke();
                        onGateClose = null;
                        break;
                    case NetPackageFlag.CLOSE:
                    case NetPackageFlag.NO_INNER_SERVER:
                        Close();
                        onServerClose?.Invoke();
                        break;
                    case NetPackageFlag.HEART:
                        if (package.body.Length > 0)
                        {
                            var id = BinaryPrimitives.ReadInt32BigEndian(package.body);
                            EndWaitHeartId(id);
                        }
                        break;
                    case NetPackageFlag.MSG:
                        onRecv?.Invoke(package.body);
                        break;
                }
            });
            onGateClose?.Invoke();
        }
    }
}
