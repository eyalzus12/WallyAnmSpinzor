using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WallyAnmSpinzor.Internal;

[SkipLocalsInit]
internal sealed class DataReader : IDisposable
{
    private readonly bool _leaveOpen;
    private readonly Stream _stream;
    private byte[] _buffer;

    internal DataReader(Stream stream, bool leaveOpen = true)
    {
        _leaveOpen = leaveOpen;
        _stream = stream;
        _buffer = new byte[8];
    }

    internal bool ReadBool()
    {
        return ReadU8() != 0;
    }

    internal async ValueTask<bool> ReadBoolAsync(CancellationToken cancellationToken = default)
    {
        return await ReadU8Async(cancellationToken) != 0;
    }

    internal byte ReadU8()
    {
        int value = _stream.ReadByte();
        if (value == -1) throw new EndOfStreamException();
        return (byte)value;
    }

    internal async ValueTask<byte> ReadU8Async(CancellationToken cancellationToken = default)
    {
        await FillBufferAsync(1, cancellationToken);
        return _buffer[0];
    }

    internal sbyte ReadI8() => (sbyte)ReadU8();
    internal async ValueTask<sbyte> ReadI8Async(CancellationToken cancellationToken = default)
    {
        return (sbyte)await ReadU8Async(cancellationToken);
    }

    internal ushort ReadU16()
    {
        FillBuffer(2);
        return BinaryPrimitives.ReadUInt16LittleEndian(_buffer);
    }

    internal async ValueTask<ushort> ReadU16Async(CancellationToken cancellationToken = default)
    {
        await FillBufferAsync(2, cancellationToken);
        return BinaryPrimitives.ReadUInt16LittleEndian(_buffer);
    }

    internal short ReadI16()
    {
        FillBuffer(2);
        return BinaryPrimitives.ReadInt16LittleEndian(_buffer);
    }

    internal async ValueTask<short> ReadI16Async(CancellationToken cancellationToken = default)
    {
        await FillBufferAsync(2, cancellationToken);
        return BinaryPrimitives.ReadInt16LittleEndian(_buffer);
    }

    internal uint ReadU32()
    {
        FillBuffer(4);
        return BinaryPrimitives.ReadUInt32LittleEndian(_buffer);
    }

    internal async ValueTask<uint> ReadU32Async(CancellationToken cancellationToken = default)
    {
        await FillBufferAsync(4, cancellationToken);
        return BinaryPrimitives.ReadUInt32LittleEndian(_buffer);
    }

    internal int ReadI32()
    {
        FillBuffer(4);
        return BinaryPrimitives.ReadInt32LittleEndian(_buffer);
    }

    internal async ValueTask<int> ReadI32Async(CancellationToken cancellationToken = default)
    {
        await FillBufferAsync(4, cancellationToken);
        return BinaryPrimitives.ReadInt32LittleEndian(_buffer);
    }

    internal float ReadF32()
    {
        FillBuffer(4);
        return BinaryPrimitives.ReadSingleLittleEndian(_buffer);
    }

    internal async ValueTask<float> ReadF32Async(CancellationToken cancellationToken = default)
    {
        await FillBufferAsync(4, cancellationToken);
        return BinaryPrimitives.ReadSingleLittleEndian(_buffer);
    }

    internal double ReadF64()
    {
        FillBuffer(8);
        return BinaryPrimitives.ReadDoubleLittleEndian(_buffer);
    }

    internal async ValueTask<double> ReadF64Async(CancellationToken cancellationToken = default)
    {
        await FillBufferAsync(8, cancellationToken);
        return BinaryPrimitives.ReadDoubleLittleEndian(_buffer);
    }

    internal string ReadStr()
    {
        ushort length = ReadU16();
        ResizeBufferToFit(length);
        FillBuffer(length);
        return Encoding.UTF8.GetString(_buffer, 0, length);
    }

    internal async ValueTask<string> ReadStrAsync(CancellationToken cancellationToken = default)
    {
        ushort length = await ReadU16Async(cancellationToken);
        ResizeBufferToFit(length);
        await FillBufferAsync(length, cancellationToken);
        return Encoding.UTF8.GetString(_buffer, 0, length);
    }

    private void FillBuffer(int bytes)
    {
        _stream.ReadExactly(_buffer.AsSpan(0, bytes));
    }

    private async ValueTask FillBufferAsync(int bytes, CancellationToken cancellationToken = default)
    {
        await _stream.ReadExactlyAsync(_buffer.AsMemory(0, bytes), cancellationToken).ConfigureAwait(false);
    }

    private void ResizeBufferToFit(ushort length)
    {
        if (_buffer.Length >= length) return;

        int newLength = _buffer.Length;
        while (length > _buffer.Length)
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