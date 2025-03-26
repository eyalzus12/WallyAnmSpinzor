using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using WallyAnmSpinzor.Internal;

namespace WallyAnmSpinzor.Version_904;

public sealed class AnmFile_904
{
    public required int Header { get; set; }
    public required Dictionary<string, AnmClass_904> Classes { get; set; }

    public static AnmFile_904 CreateFrom(Stream stream, bool leaveOpen = false)
    {
        int header = ReadHeader(stream);
        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress, leaveOpen);
        using DataReader reader = new(decompressedStream, !leaveOpen);
        return CreateFromInternal(reader, header);
    }

    public static async Task<AnmFile_904> CreateFromAsync(Stream stream, bool leaveOpen = false, CancellationToken ctoken = default)
    {
        int header = await ReadHeaderAsync(stream, ctoken);
        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress, leaveOpen);
        using DataReader reader = new(decompressedStream, !leaveOpen);
        return await CreateFromInternalAsync(reader, header, ctoken);
    }

    public void WriteTo(Stream stream, bool leaveOpen = false)
    {
        stream.PutI32(Header);
        using ZLibStream compressedStream = new(stream, CompressionLevel.SmallestSize, leaveOpen);
        WriteToInternal(compressedStream);
    }

    public async Task WriteToAsync(Stream stream, bool leaveOpen = false, CancellationToken ctoken = default)
    {
        await stream.PutI32Async(Header, ctoken);
        using ZLibStream compressedStream = new(stream, CompressionLevel.SmallestSize, leaveOpen);
        await WriteToInternalAsync(compressedStream, ctoken);
    }

    internal static int ReadHeader(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt32LittleEndian(buffer);
    }

    internal static async ValueTask<int> ReadHeaderAsync(Stream stream, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[4];
        await stream.ReadExactlyAsync(buffer, ctoken);
        return BinaryPrimitives.ReadInt32LittleEndian(buffer);
    }

    internal static AnmFile_904 CreateFromInternal(DataReader reader, int header)
    {
        Dictionary<string, AnmClass_904> classes = [];
        while (reader.ReadBool())
        {
            string key = reader.ReadStr();
            AnmClass_904 @class = AnmClass_904.CreateFrom(reader);
            classes[key] = @class;
        }

        return new()
        {
            Header = header,
            Classes = classes,
        };
    }

    internal static async Task<AnmFile_904> CreateFromInternalAsync(DataReader reader, int header, CancellationToken ctoken = default)
    {
        Dictionary<string, AnmClass_904> classes = [];
        while (await reader.ReadBoolAsync(ctoken))
        {
            string key = await reader.ReadStrAsync(ctoken);
            AnmClass_904 @class = await AnmClass_904.CreateFromAsync(reader, ctoken);
            classes[key] = @class;
        }

        return new()
        {
            Header = header,
            Classes = classes,
        };
    }

    internal void WriteToInternal(Stream stream)
    {
        foreach ((string key, AnmClass_904 @class) in Classes)
        {
            stream.PutB(true);
            stream.PutStr(key);
            @class.WriteTo(stream);
        }
        stream.PutB(false);
    }

    internal async Task WriteToInternalAsync(Stream stream, CancellationToken ctoken = default)
    {
        foreach ((string key, AnmClass_904 @class) in Classes)
        {
            await stream.PutBAsync(true, ctoken);
            await stream.PutStrAsync(key, ctoken);
            await @class.WriteToAsync(stream, ctoken);
        }
        await stream.PutBAsync(false, ctoken);
    }
}