using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WallyAnmSpinzor.Internal;

[SkipLocalsInit]
internal sealed class DataWriter : IDisposable
{
    private readonly bool _leaveOpen;
    private readonly Stream _stream;
    private byte[] _buffer;

    internal DataWriter(Stream stream, bool leaveOpen = true)
    {
        _leaveOpen = leaveOpen;
        _stream = stream;
        _buffer = new byte[8];
    }

    internal void WriteBool(bool value) => WriteU8((byte)(value ? 1 : 0));
    internal ValueTask WriteBoolAsync(bool value, CancellationToken cancellationToken = default)
    {
        return WriteU8Async((byte)(value ? 1 : 0), cancellationToken);
    }

    internal void WriteU8(byte value)
    {
        _stream.WriteByte(value);
    }

    internal async ValueTask WriteU8Async(byte value, CancellationToken cancellationToken = default)
    {
        _buffer[0] = value;
        await WriteBufferAsync(1, cancellationToken);
    }

    internal void WriteI8(sbyte value) => WriteU8((byte)value);
    internal ValueTask WriteI8Async(sbyte value, CancellationToken cancellationToken = default)
    {
        return WriteU8Async((byte)value, cancellationToken);
    }

    internal void WriteU16(ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(_buffer, value);
        WriteBuffer(2);
    }

    internal async ValueTask WriteU16Async(ushort value, CancellationToken cancellationToken = default)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(_buffer, value);
        await WriteBufferAsync(2, cancellationToken);
    }

    internal void WriteI16(short value)
    {
        BinaryPrimitives.WriteInt16LittleEndian(_buffer, value);
        WriteBuffer(2);
    }

    internal async ValueTask WriteI16Async(short value, CancellationToken cancellationToken = default)
    {
        BinaryPrimitives.WriteInt16LittleEndian(_buffer, value);
        await WriteBufferAsync(2, cancellationToken);
    }

    internal void WriteU32(uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer, value);
        WriteBuffer(4);
    }

    internal async ValueTask WriteU32Async(uint value, CancellationToken cancellationToken = default)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer, value);
        await WriteBufferAsync(4, cancellationToken);
    }

    internal void WriteI32(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(_buffer, value);
        WriteBuffer(4);
    }

    internal async ValueTask WriteI32Async(int value, CancellationToken cancellationToken = default)
    {
        BinaryPrimitives.WriteInt32LittleEndian(_buffer, value);
        await WriteBufferAsync(4, cancellationToken);
    }

    internal void WriteF32(float value)
    {
        BinaryPrimitives.WriteSingleLittleEndian(_buffer, value);
        WriteBuffer(4);
    }

    internal async ValueTask WriteF32Async(float value, CancellationToken cancellationToken = default)
    {
        BinaryPrimitives.WriteSingleLittleEndian(_buffer, value);
        await WriteBufferAsync(4, cancellationToken);
    }

    internal void WriteF64(double value)
    {
        BinaryPrimitives.WriteDoubleLittleEndian(_buffer, value);
        WriteBuffer(8);
    }

    internal async ValueTask WriteF64Async(double value, CancellationToken cancellationToken = default)
    {
        BinaryPrimitives.WriteDoubleLittleEndian(_buffer, value);
        await WriteBufferAsync(8, cancellationToken);
    }

    internal void WriteStr(string value)
    {
        int length_ = Encoding.UTF8.GetByteCount(value);
        if (length_ > ushort.MaxValue)
            throw new OverflowException("String size cannot exceed u16 limit");
        ushort length = (ushort)length_;

        WriteU16(length);
        ResizeBufferToFit(length);
        int encoded = Encoding.UTF8.GetBytes(value, _buffer);
        WriteBuffer(encoded);
    }

    internal async ValueTask WriteStrAsync(string value, CancellationToken cancellationToken = default)
    {
        int length_ = Encoding.UTF8.GetByteCount(value);
        if (length_ > ushort.MaxValue)
            throw new OverflowException("String size cannot exceed u16 limit");
        ushort length = (ushort)length_;

        await WriteU16Async(length, cancellationToken);
        ResizeBufferToFit(length);
        int encoded = Encoding.UTF8.GetBytes(value, _buffer);
        await WriteBufferAsync(encoded, cancellationToken);
    }

    private void WriteBuffer(int bytes)
    {
        _stream.Write(_buffer.AsSpan(0, bytes));
    }

    private async ValueTask WriteBufferAsync(int bytes, CancellationToken cancellationToken = default)
    {
        await _stream.WriteAsync(_buffer.AsMemory(0, bytes), cancellationToken).ConfigureAwait(false);
    }

    private void ResizeBufferToFit(ushort length)
    {
        if (_buffer.Length >= length) return;

        int newLength = _buffer.Length;
        while (length > newLength)
            newLength *= 2;
        _buffer = GC.AllocateUninitializedArray<byte>(newLength);
    }

    public void Dispose()
    {
        if (!_leaveOpen)
        {
            _stream.Dispose();
        }
    }
}