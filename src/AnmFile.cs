using System;
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
        // needs to be 8 bytes for AnmFrame
        Span<byte> buffer = stackalloc byte[8];
        int header = stream.GetI32(buffer);
        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress, leaveOpen);
        return CreateFrom(decompressedStream, header, buffer);
    }

    public void WriteTo(Stream stream, bool leaveOpen = false)
    {
        Span<byte> buffer = stackalloc byte[8];
        stream.PutI32(buffer, Header);
        using ZLibStream compressedStream = new(stream, CompressionLevel.SmallestSize, leaveOpen);
        WriteTo(compressedStream, buffer);
    }

    internal static AnmFile CreateFrom(Stream stream, int header, Span<byte> buffer)
    {
        Dictionary<string, AnmClass> classes = [];
        while (stream.GetB())
        {
            string key = stream.GetStr(buffer);
            AnmClass @class = AnmClass.CreateFrom(stream, buffer);
            classes[key] = @class;
        }

        return new()
        {
            Header = header,
            Classes = classes,
        };
    }

    internal void WriteTo(Stream stream, Span<byte> buffer)
    {
        foreach ((string key, AnmClass @class) in Classes)
        {
            stream.PutB(true);
            stream.PutStr(buffer, key);
            @class.WriteTo(stream, buffer);
        }
        stream.PutB(false);
    }
}