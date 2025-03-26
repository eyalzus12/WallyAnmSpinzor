using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace WallyAnmSpinzor.Version_904;

public class AnmFile_904
{
    public required int Header { get; set; }
    public required Dictionary<string, AnmClass_904> Classes { get; set; }

    public static AnmFile_904 CreateFrom(Stream stream, bool leaveOpen = false)
    {
        int header = stream.GetI32();
        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress, leaveOpen);
        return CreateFrom(decompressedStream, header);
    }

    public static async Task<AnmFile_904> CreateFromAsync(Stream stream, bool leaveOpen = false, CancellationToken ctoken = default)
    {
        int header = await stream.GetI32Async(ctoken);
        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress, leaveOpen);
        return await CreateFromAsync(decompressedStream, header, ctoken);
    }

    public void WriteTo(Stream stream, bool leaveOpen = false)
    {
        stream.PutI32(Header);
        using ZLibStream compressedStream = new(stream, CompressionLevel.SmallestSize, leaveOpen);
        WriteTo(compressedStream);
    }

    public async Task WriteToAsync(Stream stream, bool leaveOpen = false, CancellationToken ctoken = default)
    {
        await stream.PutI32Async(Header, ctoken);
        using ZLibStream compressedStream = new(stream, CompressionLevel.SmallestSize, leaveOpen);
        await WriteToAsync(compressedStream, ctoken);
    }

    internal static AnmFile_904 CreateFrom(Stream stream, int header)
    {
        Dictionary<string, AnmClass_904> classes = [];
        while (stream.GetB())
        {
            string key = stream.GetStr();
            AnmClass_904 @class = AnmClass_904.CreateFrom(stream);
            classes[key] = @class;
        }

        return new()
        {
            Header = header,
            Classes = classes,
        };
    }

    internal static async Task<AnmFile_904> CreateFromAsync(Stream stream, int header, CancellationToken ctoken = default)
    {
        Dictionary<string, AnmClass_904> classes = [];
        while (await stream.GetBAsync(ctoken))
        {
            string key = await stream.GetStrAsync(ctoken);
            AnmClass_904 @class = await AnmClass_904.CreateFromAsync(stream, ctoken);
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
        foreach ((string key, AnmClass_904 @class) in Classes)
        {
            stream.PutB(true);
            stream.PutStr(key);
            @class.WriteTo(stream);
        }
        stream.PutB(false);
    }

    internal async Task WriteToAsync(Stream stream, CancellationToken ctoken = default)
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