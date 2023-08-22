﻿
using Microsoft.AspNetCore.Connections;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Bedrock.Framework.Protocols
{
    public static class ProtocolReaderCreator
    {
        public static ProtocolReader CreateReader(this ConnectionContext connection)
             => new ProtocolReader(connection.Transport.Input);
    }
    public class ProtocolReader //: IAsyncDisposable
    {
        private readonly PipeReader _reader;
        private bool _disposed;

        public ProtocolReader(PipeReader reader)
        {
            _reader = reader;
        }

        public async ValueTask<ProtocolReadResult<TMessage>> ReadAsync<TMessage>(IProtocal<TMessage> protocal)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            var result = await _reader.ReadAsync();
            var buffer = result.Buffer;

            TMessage msg = default;
            SequencePosition examined = buffer.Start;
            SequencePosition consumed = examined;
            bool haveMsg = false;
            if (buffer.Length > 0)
            {
                haveMsg = protocal.TryParseMessage(buffer, ref consumed, ref examined, out msg);
                _reader.AdvanceTo(consumed, examined);
            }

            return new ProtocolReadResult<TMessage>(haveMsg, msg, result.IsCompleted);
        }

        public ValueTask DisposeAsync()
        {
            _disposed = true;
            return default;
        }
    }
}
