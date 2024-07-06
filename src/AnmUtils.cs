using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace WallyAnmSpinzor;

internal static class AnmUtils
{
    internal static bool GetB(this Stream stream) => GetU8(stream) != 0;

    internal static byte GetU8(this Stream stream)
    {
        int @byte = stream.ReadByte();
        if (@byte == -1) throw new EndOfStreamException();
        return (byte)@byte;
    }

    internal static ushort GetU16(this Stream stream, Span<byte> buffer)
    {
        stream.ReadExactly(buffer[..2]);
        return BinaryPrimitives.ReadUInt16LittleEndian(buffer[..2]);
    }

    internal static uint GetU32(this Stream stream, Span<byte> buffer)
    {
        stream.ReadExactly(buffer[..4]);
        return BinaryPrimitives.ReadUInt32LittleEndian(buffer[..4]);
    }

    internal static short GetI16(this Stream stream, Span<byte> buffer)
    {
        stream.ReadExactly(buffer[..2]);
        return BinaryPrimitives.ReadInt16LittleEndian(buffer[..2]);
    }

    internal static int GetI32(this Stream stream, Span<byte> buffer)
    {
        stream.ReadExactly(buffer[..4]);
        return BinaryPrimitives.ReadInt32LittleEndian(buffer[..4]);
    }

    internal static float GetF32(this Stream stream, Span<byte> buffer)
    {
        stream.ReadExactly(buffer[..4]);
        return BinaryPrimitives.ReadSingleLittleEndian(buffer[..4]);
    }

    internal static double GetF64(this Stream stream, Span<byte> buffer)
    {
        stream.ReadExactly(buffer[..8]);
        return BinaryPrimitives.ReadDoubleLittleEndian(buffer[..8]);
    }

    internal static string GetStr(this Stream stream, Span<byte> buffer)
    {
        ushort stringLength = GetU16(stream, buffer);
        Span<byte> stringBuffer = stackalloc byte[stringLength];
        stream.ReadExactly(stringBuffer);
        return Encoding.UTF8.GetString(stringBuffer);
    }

    internal static void PutB(this Stream stream, bool b) => stream.PutU8((byte)(b ? 1 : 0));

    internal static void PutU8(this Stream stream, byte u8)
    {
        stream.WriteByte(u8);
    }

    internal static void PutU16(this Stream stream, Span<byte> buffer, ushort u16)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(buffer[..2], u16);
        stream.Write(buffer[..2]);
    }

    internal static void PutU32(this Stream stream, Span<byte> buffer, uint u32)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[..4], u32);
        stream.Write(buffer[..4]);
    }

    internal static void PutI16(this Stream stream, Span<byte> buffer, short i16)
    {
        BinaryPrimitives.WriteInt16LittleEndian(buffer[..2], i16);
        stream.Write(buffer[..2]);
    }

    internal static void PutI32(this Stream stream, Span<byte> buffer, int i32)
    {
        BinaryPrimitives.WriteInt32LittleEndian(buffer[..4], i32);
        stream.Write(buffer[..4]);
    }

    internal static void PutF32(this Stream stream, Span<byte> buffer, float f32)
    {
        BinaryPrimitives.WriteSingleLittleEndian(buffer[..4], f32);
        stream.Write(buffer[..4]);
    }

    internal static void PutF64(this Stream stream, Span<byte> buffer, double f64)
    {
        BinaryPrimitives.WriteDoubleLittleEndian(buffer[..8], f64);
        stream.Write(buffer[..8]);
    }

    internal static void PutStr(this Stream stream, Span<byte> buffer, string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        ushort len = (ushort)bytes.Length;
        stream.PutU16(buffer, len);
        stream.Write(bytes);
    }
}