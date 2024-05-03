using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace WallyAnmSpinzor;

public class ByteReader(Stream stream, bool leaveOpen = false) : IDisposable
{
    private bool _disposedValue;

    public byte[] ReadBytes(int count)
    {
        Span<byte> buffer = stackalloc byte[count];
        stream.ReadExactly(buffer);
        return buffer.ToArray();
    }

    public string ReadString(int length)
    {
        Span<byte> buffer = stackalloc byte[length];
        stream.ReadExactly(buffer);
        return Encoding.UTF8.GetString(buffer);
    }

    public byte ReadU8() => (byte)stream.ReadByte();
    public sbyte ReadI8() => (sbyte)ReadU8();

    public ushort ReadU16LE()
    {
        Span<byte> buffer = stackalloc byte[2];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
    }
    public short ReadI16LE() => (short)ReadU16LE();

    public ushort ReadU16BE()
    {
        Span<byte> buffer = stackalloc byte[2];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }
    public short ReadI16BE() => (short)ReadU16BE();

    public uint ReadU24LE()
    {
        Span<byte> buffer = stackalloc byte[3];
        stream.ReadExactly(buffer);
        return (uint)((buffer[0] << 0x10) | (buffer[1] << 0x08) | (buffer[2] << 0x00));
    }
    public int ReadI24LE() => (int)((ReadU24LE() << 8) >> 8); // the shift is for sign extension

    public uint ReadU24BE()
    {
        Span<byte> buffer = stackalloc byte[3];
        stream.ReadExactly(buffer);
        return (uint)((buffer[2] << 0x10) | (buffer[1] << 0x08) | (buffer[0] << 0x00));
    }
    public int ReadI24BE() => (int)((ReadU24BE() << 8) >> 8); // the shift is for sign extension

    public uint ReadU32LE()
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
    }
    public int ReadI32LE() => (int)ReadU32LE();

    public uint ReadU32BE()
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt32BigEndian(buffer);
    }
    public int ReadI32BE() => (int)ReadU32BE();

    public ulong ReadU64LE()
    {
        Span<byte> buffer = stackalloc byte[8];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
    }
    public long ReadI64LE() => (long)ReadU64LE();

    public ulong ReadU64BE()
    {
        Span<byte> buffer = stackalloc byte[8];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt64BigEndian(buffer);
    }
    public long ReadI64BE() => (long)ReadU64BE();

    public UInt128 ReadU128LE()
    {
        Span<byte> buffer = stackalloc byte[16];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt128LittleEndian(buffer);
    }
    public Int128 ReadI128LE() => (Int128)ReadU128LE();

    public UInt128 ReadU128BE()
    {
        Span<byte> buffer = stackalloc byte[16];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt128BigEndian(buffer);
    }
    public Int128 ReadI128BE() => (Int128)ReadU128BE();

    public Half ReadF16LE()
    {
        Span<byte> buffer = stackalloc byte[2];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadHalfLittleEndian(buffer);
    }

    public Half ReadF16BE()
    {
        Span<byte> buffer = stackalloc byte[2];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadHalfBigEndian(buffer);
    }

    public float ReadF32LE()
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadSingleLittleEndian(buffer);
    }

    public float ReadF32BE()
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadSingleBigEndian(buffer);
    }

    public double ReadF64LE()
    {
        Span<byte> buffer = stackalloc byte[8];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
    }

    public double ReadF64BE()
    {
        Span<byte> buffer = stackalloc byte[8];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadDoubleBigEndian(buffer);
    }

    public string ReadFlashString() => ReadString(ReadU16LE());

    private ulong ReadFlashVarInteger(int bytes)
    {
        ulong result = ReadU8();
        for (int offset = 7; offset <= 7 * bytes; offset += 7)
        {
            uint checkedBit = 1u << offset;
            if ((result & checkedBit) == 0)
                return result;
            result &= checkedBit - 1;
            result |= (ulong)ReadU8() << offset;
        }
        return result;
    }
    public uint ReadFlashVarU32() => (uint)ReadFlashVarInteger(5);
    // hacky way to get sign extension
    public int ReadFlashVarI32() => BitConverter.ToInt32(BitConverter.GetBytes(ReadFlashVarU32()));
    public uint ReadFlashVarU30() => (uint)ReadFlashVarInteger(4);

    public bool CanRead => stream.CanRead;
    public long Position => stream.Position;
    public long Length => stream.Length;


    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (!leaveOpen) stream.Dispose();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}