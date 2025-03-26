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
    internal static bool GetB(this Stream stream) => GetU8(stream) != 0;
    internal static async ValueTask<bool> GetBAsync(this Stream stream, CancellationToken ctoken = default)
    {
        return await GetU8Async(stream, ctoken) != 0;
    }

    internal static byte GetU8(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[1];
        stream.ReadExactly(buffer);
        return buffer[0];
    }

    internal static async ValueTask<byte> GetU8Async(this Stream stream, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[1];
        await stream.ReadExactlyAsync(buffer, ctoken);
        return buffer[0];
    }

    internal static ushort GetU16(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[2];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
    }

    internal static async ValueTask<ushort> GetU16Async(this Stream stream, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[2];
        await stream.ReadExactlyAsync(buffer, ctoken);
        return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
    }

    internal static uint GetU32(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
    }

    internal static async ValueTask<uint> GetU32Async(this Stream stream, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[4];
        await stream.ReadExactlyAsync(buffer, ctoken);
        return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
    }

    internal static short GetI16(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[2];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt16LittleEndian(buffer);
    }

    internal static async ValueTask<short> GetI16Async(this Stream stream, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[2];
        await stream.ReadExactlyAsync(buffer, ctoken);
        return BinaryPrimitives.ReadInt16LittleEndian(buffer);
    }

    internal static int GetI32(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt32LittleEndian(buffer);
    }

    internal static async ValueTask<int> GetI32Async(this Stream stream, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[4];
        await stream.ReadExactlyAsync(buffer, ctoken);
        return BinaryPrimitives.ReadInt32LittleEndian(buffer);
    }

    internal static float GetF32(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadSingleLittleEndian(buffer);
    }

    internal static async ValueTask<float> GetF32Async(this Stream stream, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[4];
        await stream.ReadExactlyAsync(buffer, ctoken);
        return BinaryPrimitives.ReadSingleLittleEndian(buffer);
    }

    internal static double GetF64(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[8];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
    }

    internal static async ValueTask<double> GetF64Async(this Stream stream, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[8];
        await stream.ReadExactlyAsync(buffer, ctoken);
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

    internal static async ValueTask<string> GetStrAsync(this Stream stream, CancellationToken ctoken = default)
    {
        ushort stringLength = await GetU16Async(stream, ctoken);
        byte[] stringBuffer = GC.AllocateUninitializedArray<byte>(stringLength);
        await stream.ReadExactlyAsync(stringBuffer, ctoken);
        return Encoding.UTF8.GetString(stringBuffer);
    }

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