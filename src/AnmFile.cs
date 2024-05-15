using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace WallyAnmSpinzor;

public class AnmFile
{
    public required int Header { get; set; }
    public required Dictionary<string, AnmGroup> Groups { get; set; }

    public static AnmFile CreateFrom(Stream stream)
    {
        // needs to be 8 bytes for AnmFrame
        Span<byte> buffer = stackalloc byte[8];

        stream.ReadExactly(buffer[..4]);
        int header = BinaryPrimitives.ReadInt32LittleEndian(buffer[..4]);

        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress);
        return CreateFrom(decompressedStream, header, buffer);
    }

    internal static AnmFile CreateFrom(Stream stream, int header, Span<byte> buffer)
    {
        Dictionary<string, AnmGroup> groups = [];
        stream.ReadExactly(buffer[..1]);
        while (buffer[0] != 0)
        {
            string name = ReadString(stream, buffer);
            AnmGroup group = AnmGroup.CreateFrom(stream, buffer);
            groups[name] = group;

            stream.ReadExactly(buffer[..1]);
        }

        return new()
        {
            Header = header,
            Groups = groups,
        };
    }

    private static string ReadString(Stream stream, Span<byte> buffer)
    {
        stream.ReadExactly(buffer[..2]);
        ushort stringLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[..2]);
        Span<byte> stringBuffer = stackalloc byte[stringLength];
        stream.ReadExactly(stringBuffer);
        return Encoding.UTF8.GetString(stringBuffer);
    }
}