﻿
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Bedrock.Framework;
using MessagePack;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class KcpTcpClientSocket : IKcpSocket
{
    public delegate void ReceiveFunc(TempNetPackage package);
    CancellationTokenSource cancelSrc = new CancellationTokenSource();
    ConnectionContext context;
    PipeWriter netWriter;
    PipeReader netReader;
    public long NetId { get; set; }
    public int ServerId { get; set; }

    public KcpTcpClientSocket(int serverId)
    {
        this.ServerId = serverId;
    }

    public async Task<bool> Connect(string ip, int port, long netId = 0)
    {
        this.NetId = netId;
        context = await new SocketConnection(AddressFamily.InterNetwork, ip, port).StartAsync();
        if (context == null)
            return false;

        netReader = context.Transport.Input;
        netWriter = context.Transport.Output;
        Send(new TempNetPackage(NetPackageFlag.SYN, NetId, ServerId));

        bool connectResult = false;

        //等待连接消息
        while (!IsClose())
        {
            try
            {
                var result = await netReader.ReadAsync();
                if (TryParseNetPack(result.Buffer, (pack) =>
                {
                    connectResult = false;
                    if (pack.flag == NetPackageFlag.ACK)
                    {
                        Debug.Log($"连接成功..{pack.netId}");
                        connectResult = true;
                        NetId = pack.netId;
                    }
                    if (pack.flag == NetPackageFlag.NO_GATE_CONNECT && pack.innerServerId == 0)
                    {
                        Debug.LogError($"内部服务器{ServerId}已关闭或者不存在，连接失败...");
                    }
                }))
                    break;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        return connectResult;
    }

    void StartGateHeart()
    {
        Task.Run(async () =>
        {
            while (!cancelSrc.IsCancellationRequested)
            {
                try
                {
                    Send(new TempNetPackage(NetPackageFlag.GATE_HEART, NetId, ServerId));
                    await Task.Delay(2000, cancelSrc.Token);
                }
                catch
                {
                    break;
                }
            }
        });
    }

    async Task ReadPackAsync(ReceiveFunc onPack)
    {
        while (!IsClose())
        {
            try
            {
                var result = await netReader.ReadAsync();
                //Debug.Log($"read tcp len:{result.Buffer.Length}");
                if (result.Buffer.Length > 0)
                {
                    TryParseNetPack(result.Buffer, onPack);
                }
            }
            catch (Exception e)
            {
                break;
            }
        }
        Close();
    }

    bool TryParseNetPack(in ReadOnlySequence<byte> input, ReceiveFunc onPack)
    {
        SequencePosition examined = input.Start;
        SequencePosition consumed = examined;
        var reader = new SequenceReader<byte>(input);

        if (!reader.TryReadBigEndian(out int msgLen))
        {
            examined = input.End; //告诉read task，到这里为止还不满足一个消息的长度，继续等待更多数据 
            netReader.AdvanceTo(consumed, examined);
            return false;
        }

        if (msgLen < 13)
        {
            throw new ProtocalParseErrorException($"从客户端接收的包大小异常,{msgLen} {reader.Remaining}:至少大于8个字节");
        }
        else if (msgLen > 1500)
        {
            throw new ProtocalParseErrorException("从客户端接收的包大小超过限制：" + msgLen + "字节，最大值1500字节");
        }

        if (reader.Remaining < msgLen)
        {
            examined = input.End;
            netReader.AdvanceTo(consumed, examined);
            return false;
        }

        reader.TryRead(out byte flag);
        reader.TryReadBigEndian(out long netId);
        reader.TryReadBigEndian(out int serverId);
        var dataLen = msgLen - 13;
        if (dataLen > 0)
        {
            var payload = input.Slice(reader.Position, dataLen);
            Span<byte> data = stackalloc byte[dataLen];
            payload.CopyTo(data);
            consumed = examined = payload.End;
            onPack(new TempNetPackage(flag, netId, serverId, data));
        }
        else
        {
            consumed = examined = reader.Position;
            onPack(new TempNetPackage(flag, netId, serverId));
        }
        netReader.AdvanceTo(consumed, examined);
        return true;
    }

    public bool IsClose()
    {
        return context == null || !Application.isPlaying;
    }

    public void Close()
    {
        context.Abort();
        cancelSrc?.Cancel();
        context = null;
    }

    public void Send(TempNetPackage package)
    {
        Span<byte> target = stackalloc byte[package.Length + 4];
        int offset = 0;
        target.Write(package.Length, ref offset);
        target.Write(package, ref offset);
        netWriter.Write(target);
        //Debug.Log($"写tcp pack:{package.ToString()}");
        netWriter.FlushAsync();
    }

    public async Task StartRecv(OnReceiveNetPackFunc onRecv, Action onGateClose, Action onServerClose)
    {
        StartGateHeart();
        await ReadPackAsync((package) =>
        {
            if (package.netId != NetId)
                return;
            switch (package.flag)
            {
                case NetPackageFlag.NO_GATE_CONNECT:
                    Debug.LogError("gate 断开连接...");
                    onGateClose?.Invoke();
                    onGateClose = null;
                    Close();
                    break;
                case NetPackageFlag.CLOSE:
                    Debug.LogError("server 断开连接...");
                    onServerClose?.Invoke();
                    Close();
                    break;
                case NetPackageFlag.MSG:
                    onRecv?.Invoke(package.body);
                    break;
            }
        });
        //网络连接主动断开，则认为是网关断开
        onGateClose?.Invoke();
    }
}