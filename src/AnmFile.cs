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
    public required Dictionary<string, AnmGroup> AnimationGroups { get; set; }

    public static AnmFile CreateFrom(Stream stream)
    {
        Span<byte> headerBytes = stackalloc byte[4];
        stream.ReadExactly(headerBytes[..4]);
        int header = BinaryPrimitives.ReadInt32LittleEndian(headerBytes);

        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress);
        return CreateFrom(decompressedStream, header);
    }

    internal static AnmFile CreateFrom(Stream stream, int header)
    {
        Span<byte> buffer = stackalloc byte[2];
        byte[] stringBuffer;

        Dictionary<string, AnmGroup> animationGroups = [];
        stream.ReadExactly(buffer[..1]);
        while (buffer[0] != 0)
        {
            stream.ReadExactly(buffer[..2]);
            ushort nameLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[..2]);
            stringBuffer = new byte[nameLength];
            stream.ReadExactly(stringBuffer);
            string name = Encoding.UTF8.GetString(stringBuffer);

            AnmGroup group = AnmGroup.CreateFrom(stream);
            animationGroups[name] = group;

            stream.ReadExactly(buffer[..1]);
        }

        return new()
        {
            Header = header,
            AnimationGroups = animationGroups,
        };
    }
}