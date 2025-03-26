using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace WallyAnmSpinzor;

public sealed class AnmFile
{
    public required int Header { get; set; }
    public required Dictionary<string, AnmClass> Classes { get; set; }

    public static AnmFile CreateFrom(Stream stream, bool leaveOpen = false)
    {
        int header = stream.GetI32();
        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress, leaveOpen);
        return CreateFromInternal(decompressedStream, header);
    }

    public static async Task<AnmFile> CreateFromAsync(Stream stream, bool leaveOpen = false, CancellationToken ctoken = default)
    {
        int header = await stream.GetI32Async(ctoken);
        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress, leaveOpen);
        return await CreateFromInternalAsync(decompressedStream, header, ctoken);
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

    internal static AnmFile CreateFromInternal(Stream stream, int header)
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

    internal static async Task<AnmFile> CreateFromInternalAsync(Stream stream, int header, CancellationToken ctoken = default)
    {
        Dictionary<string, AnmClass> classes = [];
        while (await stream.GetBAsync(ctoken))
        {
            string key = await stream.GetStrAsync(ctoken);
            AnmClass @class = await AnmClass.CreateFromAsync(stream, ctoken);
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
        foreach ((string key, AnmClass @class) in Classes)
        {
            stream.PutB(true);
            stream.PutStr(key);
            @class.WriteTo(stream);
        }
        stream.PutB(false);
    }

    internal async Task WriteToInternalAsync(Stream stream, CancellationToken ctoken = default)
    {
        foreach ((string key, AnmClass @class) in Classes)
        {
            await stream.PutBAsync(true, ctoken);
            await stream.PutStrAsync(key, ctoken);
            await @class.WriteToAsync(stream, ctoken);
        }
        await stream.PutBAsync(false, ctoken);
    }
}