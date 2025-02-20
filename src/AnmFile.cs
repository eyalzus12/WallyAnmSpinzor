using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

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

    public void WriteTo(Stream stream, bool leaveOpen = false)
    {
        stream.PutI32(Header);
        using ZLibStream compressedStream = new(stream, CompressionLevel.SmallestSize, leaveOpen);
        WriteTo(compressedStream);
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
}