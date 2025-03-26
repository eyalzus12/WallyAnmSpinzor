using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WallyAnmSpinzor;

[SkipLocalsInit]
internal static class AnmUtils
{
    internal static void PutB(this Stream stream, bool b) => PutU8(stream, (byte)(b ? 1 : 0));
    internal static ValueTask PutBAsync(this Stream stream, bool b, CancellationToken ctoken = default)
    {
        return PutU8Async(stream, (byte)(b ? 1 : 0), ctoken);
    }

    internal static void PutU8(this Stream stream, byte u8)
    {
        stream.WriteByte(u8);
    }

    internal static ValueTask PutU8Async(this Stream stream, byte u8, CancellationToken ctoken = default)
    {
        byte[] buffer = [u8];
        return stream.WriteAsync(buffer, ctoken);
    }

    internal static void PutU16(this Stream stream, ushort u16)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(buffer, u16);
        stream.Write(buffer);
    }

    internal static ValueTask PutU16Async(this Stream stream, ushort u16, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(buffer, u16);
        return stream.WriteAsync(buffer, ctoken);
    }

    internal static void PutU32(this Stream stream, uint u32)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(buffer, u32);
        stream.Write(buffer);
    }

    internal static ValueTask PutU32Async(this Stream stream, uint u32, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(buffer, u32);
        return stream.WriteAsync(buffer, ctoken);
    }

    internal static void PutI8(this Stream stream, sbyte i8) => PutU8(stream, (byte)i8);
    internal static ValueTask PutI8Async(this Stream stream, sbyte i8, CancellationToken ctoken = default)
    {
        return PutU8Async(stream, (byte)i8, ctoken);
    }

    internal static void PutI16(this Stream stream, short i16)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteInt16LittleEndian(buffer, i16);
        stream.Write(buffer);
    }

    internal static ValueTask PutI16Async(this Stream stream, short i16, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[2];
        BinaryPrimitives.WriteInt16LittleEndian(buffer, i16);
        return stream.WriteAsync(buffer, ctoken);
    }

    internal static void PutI32(this Stream stream, int i32)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, i32);
        stream.Write(buffer);
    }

    internal static ValueTask PutI32Async(this Stream stream, int i32, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, i32);
        return stream.WriteAsync(buffer, ctoken);
    }

    internal static void PutF32(this Stream stream, float f32)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteSingleLittleEndian(buffer, f32);
        stream.Write(buffer);
    }

    internal static ValueTask PutF32Async(this Stream stream, float f32, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[4];
        BinaryPrimitives.WriteSingleLittleEndian(buffer, f32);
        return stream.WriteAsync(buffer, ctoken);
    }

    internal static void PutF64(this Stream stream, double f64)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteDoubleLittleEndian(buffer, f64);
        stream.Write(buffer);
    }

    internal static ValueTask PutF64Async(this Stream stream, double f64, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[8];
        BinaryPrimitives.WriteDoubleLittleEndian(buffer, f64);
        return stream.WriteAsync(buffer, ctoken);
    }

    internal static void PutStr(this Stream stream, string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        ushort len = (ushort)bytes.Length;
        stream.PutU16(len);
        stream.Write(bytes);
    }

    internal static async ValueTask PutStrAsync(this Stream stream, string str, CancellationToken ctoken = default)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        ushort len = (ushort)bytes.Length;
        await PutU16Async(stream, len, ctoken);
        await stream.WriteAsync(bytes, ctoken);
    }
}