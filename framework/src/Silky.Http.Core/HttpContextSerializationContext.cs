using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.ResponseCompression;

namespace Silky.Http.Core;

internal sealed class HttpContextSerializationContext
{
    private readonly HttpContextServerCallContext _serverCallContext;

    private InternalState _state;
    private int? _payloadLength;
    private ICompressionProvider? _compressionProvider;
    private ArrayBufferWriter<byte>? _bufferWriter;

    public PipeWriter ResponseBufferWriter { get; set; } = default!;

    private enum InternalState : byte
    {
        Initialized,
        CompleteArray,
        IncompleteBufferWriter,
        CompleteBufferWriter,
    }

    private bool DirectSerializationSupported => _compressionProvider == null && _payloadLength != null;

    public HttpContextSerializationContext(HttpContextServerCallContext serverCallContext)
    {
        _serverCallContext = serverCallContext;
    }

    public void Reset()
    {
        _compressionProvider = ResolveCompressionProvider();

        _payloadLength = null;
        if (_bufferWriter != null)
        {
            // Reuse existing buffer writer
            _bufferWriter.Clear();
        }

        _state = InternalState.Initialized;
    }

    private ICompressionProvider? ResolveCompressionProvider()
    {
        // if (_serverCallContext.ResponseGrpcEncoding != null &&
        //     GrpcProtocolHelpers.CanWriteCompressed(_serverCallContext.WriteOptions) &&
        //     _serverCallContext.Options.CompressionProviders.TryGetValue(_serverCallContext.ResponseGrpcEncoding, out var compressionProvider))
        // {
        //     return compressionProvider;
        // }

        return null;
    }

    public void SetPayloadLength(int payloadLength)
    {
        switch (_state)
        {
            case InternalState.Initialized:
                _payloadLength = payloadLength;
                break;
            default:
                ThrowInvalidState(_state);
                break;
        }
    }

    public void Complete(byte[] payload)
    {
        switch (_state)
        {
            case InternalState.Initialized:
                _state = InternalState.CompleteArray;

                // GrpcServerLog.SerializedMessage(_serverCallContext.Logger, _serverCallContext.ResponseType, payload.Length);
                WriteMessage(payload);
                break;
            default:
                ThrowInvalidState(_state);
                break;
        }
    }

    private static void WriteHeader(PipeWriter pipeWriter, int length, bool compress)
    {
        const int MessageDelimiterSize = 4; // how many bytes it takes to encode "Message-Length"
        const int HeaderSize = MessageDelimiterSize + 1; // message length + compression flag

        var headerData = pipeWriter.GetSpan(HeaderSize);

        // Compression flag
        headerData[0] = compress ? (byte)1 : (byte)0;

        // Message length
        BinaryPrimitives.WriteUInt32BigEndian(headerData.Slice(1), (uint)length);

        pipeWriter.Advance(HeaderSize);
    }

    public IBufferWriter<byte> GetBufferWriter()
    {
        switch (_state)
        {
            case InternalState.Initialized:
                // When writing directly to the buffer the header with message size needs to be written first
                if (DirectSerializationSupported)
                {
                    Debug.Assert(_payloadLength != null, "A payload length is required for direct serialization.");

                    //EnsureMessageSizeAllowed(_payloadLength.Value);

                    WriteHeader(ResponseBufferWriter, _payloadLength.Value, compress: false);
                }

                _state = InternalState.IncompleteBufferWriter;
                return ResolveBufferWriter();
            case InternalState.IncompleteBufferWriter:
                return ResolveBufferWriter();
            default:
                ThrowInvalidState(_state);
                return default!;
        }
    }

    private IBufferWriter<byte> ResolveBufferWriter()
    {
        return DirectSerializationSupported
            ? (IBufferWriter<byte>)ResponseBufferWriter
            : _bufferWriter ??= new ArrayBufferWriter<byte>();
    }

    // private void EnsureMessageSizeAllowed(int payloadLength)
    // {
    //     if (payloadLength > _serverCallContext.Options.MaxSendMessageSize)
    //     {
    //         throw new RpcException(SendingMessageExceedsLimitStatus);
    //     }
    // }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowInvalidState(InternalState state)
        => throw new InvalidOperationException("Not valid in the current state: " + state.ToString());

    public void Complete()
    {
        switch (_state)
        {
            case InternalState.IncompleteBufferWriter:
                _state = InternalState.CompleteBufferWriter;

                if (!DirectSerializationSupported)
                {
                    Debug.Assert(_bufferWriter != null, "Buffer writer has been set to get to this state.");

                    var data = _bufferWriter.WrittenSpan;

                    // GrpcServerLog.SerializedMessage(_serverCallContext.Logger, _serverCallContext.ResponseType, data.Length);
                    WriteMessage(data);
                }
                else
                {
                    // GrpcServerLog.SerializedMessage(_serverCallContext.Logger, _serverCallContext.ResponseType, _payloadLength.GetValueOrDefault());
                }

                break;
            default:
                ThrowInvalidState(_state);
                break;
        }
    }

    private void WriteMessage(ReadOnlySpan<byte> data)
    {
        //EnsureMessageSizeAllowed(data.Length);

        if (_compressionProvider != null)
        {
            data = CompressMessage(data);
        }

        WriteHeader(ResponseBufferWriter, data.Length, compress: _compressionProvider != null);
        ResponseBufferWriter.Write(data);
    }

    private ReadOnlySpan<byte> CompressMessage(ReadOnlySpan<byte> messageData)
    {
        Debug.Assert(_compressionProvider != null, "Compression provider is not null to get here.");

        // GrpcServerLog.CompressingMessage(_serverCallContext.Logger, _compressionProvider.EncodingName);

        var output = new NonDisposableMemoryStream();

        // Compression stream must be disposed before its content is read.
        // GZipStream writes final Adler32 at the end of the stream on dispose.
        // using (var compressionStream =
        //        _compressionProvider.CreateCompressionStream(output,
        //            _serverCallContext.Options.ResponseCompressionLevel))
        // {
        //     compressionStream.Write(messageData);
        // }

        return output.GetBuffer().AsSpan(0, (int)output.Length);
    }
}