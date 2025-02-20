using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace WallyAnmSpinzor;

[SkipLocalsInit]
internal static class AnmUtils
{
    internal static bool GetB(this Stream stream) => GetU8(stream) != 0;

    internal static byte GetU8(this Stream stream)
    {
        int @byte = stream.ReadByte();
        if (@byte == -1) throw new EndOfStreamException();
        return (byte)@byte;
    }

    internal static ushort GetU16(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[2];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
    }

    internal static uint GetU32(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
    }

    internal static short GetI16(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[2];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt16LittleEndian(buffer);
    }

    internal static int GetI32(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt32LittleEndian(buffer);
    }

    internal static float GetF32(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadSingleLittleEndian(buffer);
    }

    internal static double GetF64(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[8];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
    }

    internal static string GetStr(this Stream stream)
    {
        ushort stringLength = GetU16(stream);
        Span<byte> stringBuffer = stringLength > 1024
            ? GC.AllocateUninitializedArray<byte>(stringLength)
            : (stackalloc byte[1024])[..stringLength];

        stream.ReadExactly(stringBuffer);
        return Encoding.UTF8.GetString(stringBuffer);
    }

    internal static void PutB(this Stream stream, bool b) => stream.PutU8((byte)(b ? 1 : 0));

    internal static void PutU8(this Stream stream, byte u8)
    {
        stream.WriteByte(u8);
    }

    internal static void PutU16(this Stream stream, ushort u16)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(buffer, u16);
        stream.Write(buffer);
    }

    internal static void PutU32(this Stream stream, uint u32)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(buffer, u32);
        stream.Write(buffer);
    }

    internal static void PutI16(this Stream stream, short i16)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteInt16LittleEndian(buffer, i16);
        stream.Write(buffer);
    }

    internal static void PutI32(this Stream stream, int i32)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, i32);
        stream.Write(buffer);
    }

    internal static void PutF32(this Stream stream, float f32)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteSingleLittleEndian(buffer, f32);
        stream.Write(buffer);
    }

    internal static void PutF64(this Stream stream, double f64)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteDoubleLittleEndian(buffer, f64);
        stream.Write(buffer);
    }

    internal static void PutStr(this Stream stream, string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        ushort len = (ushort)bytes.Length;
        stream.PutU16(len);
        stream.Write(bytes);
    }
}