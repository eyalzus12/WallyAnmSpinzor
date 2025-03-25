using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace WallyAnmSpinzor;

public class AnmFile
{
    public required int Header { get; set; }
    public required Dictionary<string, AnmClass> Classes { get; set; }

    public static AnmFile CreateFrom(Stream stream, bool leaveOpen = false)
    {
        int header = stream.GetI32();
        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress, leaveOpen);
        return CreateFrom(decompressedStream, header);
    }

    public static async Task<AnmFile> CreateFromAsync(Stream stream, bool leaveOpen = false, CancellationToken ctoken = default)
    {
        // 1024 for short strings
        Memory<byte> buffer = GC.AllocateUninitializedArray<byte>(1024);

        int header = await stream.GetI32Async(buffer, ctoken);
        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress, leaveOpen);
        return await CreateFromAsync(decompressedStream, header, buffer, ctoken);
    }

    public void WriteTo(Stream stream, bool leaveOpen = false)
    {
        stream.PutI32(Header);
        using ZLibStream compressedStream = new(stream, CompressionLevel.SmallestSize, leaveOpen);
        WriteTo(compressedStream);
    }

    public async Task WriteToAsync(Stream stream, bool leaveOpen = false, CancellationToken ctoken = default)
    {
        // 8 bytes for double
        Memory<byte> buffer = GC.AllocateUninitializedArray<byte>(8);

        await stream.PutI32Async(Header, buffer, ctoken);
        using ZLibStream compressedStream = new(stream, CompressionLevel.SmallestSize, leaveOpen);
        await WriteToAsync(compressedStream, buffer, ctoken);
    }

    internal static AnmFile CreateFrom(Stream stream, int header)
    {
        Dictionary<string, AnmClass> classes = [];
        while (stream.GetB())
        {
            string key = stream.GetStr();
            AnmClass @class = AnmClass.CreateFrom(stream);
            classes[key] = @class;
        }

        return new()
        {
            Header = header,
            Classes = classes,
        };
    }

    internal static async Task<AnmFile> CreateFromAsync(Stream stream, int header, Memory<byte> buffer, CancellationToken ctoken = default)
    {
        Dictionary<string, AnmClass> classes = [];
        while (await stream.GetBAsync(buffer, ctoken))
        {
            string key = await stream.GetStrAsync(buffer, ctoken);
            AnmClass @class = await AnmClass.CreateFromAsync(stream, buffer, ctoken);
            classes[key] = @class;
        }

        return new()
        {
            Header = header,
            Classes = classes,
        };
    }

    internal void WriteTo(Stream stream)
    {
        foreach ((string key, AnmClass @class) in Classes)
        {
            stream.PutB(true);
            stream.PutStr(key);
            @class.WriteTo(stream);
        }
        stream.PutB(false);
    }

    internal async Task WriteToAsync(Stream stream, Memory<byte> buffer, CancellationToken ctoken = default)
    {
        foreach ((string key, AnmClass @class) in Classes)
        {
            await stream.PutBAsync(true, buffer, ctoken);
            await stream.PutStrAsync(key, buffer, ctoken);
            await @class.WriteToAsync(stream, buffer, ctoken);
        }
        await stream.PutBAsync(false, buffer, ctoken);
    }
}