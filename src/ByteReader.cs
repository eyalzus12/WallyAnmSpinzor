using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace WallyAnmSpinzor;

public class ByteReader(Stream stream, bool leaveOpen = false) : IDisposable
{
    private bool _disposedValue;

    private byte[] ExtractBytes<T>() where T : unmanaged
    {
        byte[] buffer = new byte[Unsafe.SizeOf<T>()];
        stream.ReadExactly(buffer, 0, buffer.Length);
        return buffer;
    }

    public byte[] ReadBytes(int count)
    {
        byte[] buffer = new byte[count];
        stream.ReadExactly(buffer, 0, buffer.Length);
        return buffer;
    }

    public string ReadString(int length) => Encoding.UTF8.GetString(ReadBytes(length));

    private static ulong FromBE(byte[] buffer)
    {
        ulong result = 0;
        for (int i = 0; i < buffer.Length; ++i)
        {
            result |= (ulong)buffer[i] << (8 * i);
        }
        return result;
    }

    private static ulong FromLE(byte[] buffer)
    {
        ulong result = 0;
        for (int i = 0; i < buffer.Length; ++i)
        {
            result <<= 8;
            result |= buffer[i];
        }
        return result;
    }

    public byte ReadU8() => ExtractBytes<byte>()[0];
    public sbyte ReadI8() => (sbyte)ReadU8();

    public ushort ReadU16LE() => BinaryPrimitives.ReadUInt16LittleEndian(ExtractBytes<ushort>());
    public ushort ReadU16BE() => BinaryPrimitives.ReadUInt16BigEndian(ExtractBytes<ushort>());
    public short ReadI16LE() => BinaryPrimitives.ReadInt16LittleEndian(ExtractBytes<short>());
    public short ReadI16BE() => BinaryPrimitives.ReadInt16BigEndian(ExtractBytes<short>());

    public uint ReadU24LE() => (uint)FromLE(ReadBytes(3));
    public uint ReadU24BE() => (uint)FromBE(ReadBytes(3));
    public int ReadI24LE() => (int)FromLE(ReadBytes(3));
    public int ReadI24BE() => (int)FromBE(ReadBytes(3));

    public uint ReadU32LE() => BinaryPrimitives.ReadUInt32LittleEndian(ExtractBytes<uint>());
    public uint ReadU32BE() => BinaryPrimitives.ReadUInt32BigEndian(ExtractBytes<uint>());
    public int ReadI32LE() => BinaryPrimitives.ReadInt32LittleEndian(ExtractBytes<int>());
    public int ReadI32BE() => BinaryPrimitives.ReadInt32BigEndian(ExtractBytes<int>());

    public ulong ReadU64LE() => BinaryPrimitives.ReadUInt64LittleEndian(ExtractBytes<ulong>());
    public ulong ReadU64BE() => BinaryPrimitives.ReadUInt64BigEndian(ExtractBytes<ulong>());
    public long ReadI64LE() => BinaryPrimitives.ReadInt64LittleEndian(ExtractBytes<long>());
    public long ReadI64BE() => BinaryPrimitives.ReadInt64BigEndian(ExtractBytes<long>());

    public UInt128 ReadU128LE() => BinaryPrimitives.ReadUInt128LittleEndian(ExtractBytes<UInt128>());
    public UInt128 ReadU128BE() => BinaryPrimitives.ReadUInt128BigEndian(ExtractBytes<UInt128>());
    public Int128 ReadI128LE() => BinaryPrimitives.ReadInt128LittleEndian(ExtractBytes<Int128>());
    public Int128 ReadI128BE() => BinaryPrimitives.ReadInt128BigEndian(ExtractBytes<Int128>());

    public Half ReadF16LE() => BinaryPrimitives.ReadHalfLittleEndian(ExtractBytes<Half>());
    public Half ReadF16BE() => BinaryPrimitives.ReadHalfBigEndian(ExtractBytes<Half>());

    public float ReadF32LE() => BinaryPrimitives.ReadSingleLittleEndian(ExtractBytes<float>());
    public float ReadF32BE() => BinaryPrimitives.ReadSingleBigEndian(ExtractBytes<float>());

    public double ReadF64LE() => BinaryPrimitives.ReadDoubleLittleEndian(ExtractBytes<double>());
    public double ReadF64BE() => BinaryPrimitives.ReadDoubleBigEndian(ExtractBytes<double>());

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