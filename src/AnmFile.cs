using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace WallyAnmSpinzor;

public class AnmFile
{
    public required int Header { get; set; }
    public required Dictionary<string, AnmGroup> AnimationGroups { get; set; }

    public static AnmFile CreateFrom(Stream stream)
    {
        byte[] headerBytes = new byte[4];
        stream.ReadExactly(headerBytes, 0, 4);
        int header = BinaryPrimitives.ReadInt32LittleEndian(headerBytes);

        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress);
        using ByteReader br = new(decompressedStream);
        return CreateFrom(br, header);
    }

    internal static AnmFile CreateFrom(ByteReader br, int header)
    {
        Dictionary<string, AnmGroup> animationGroups = [];
        while (br.ReadU8() != 0)
        {
            string name = br.ReadFlashString();
            AnmGroup group = AnmGroup.CreateFrom(br);
            animationGroups[name] = group;
        }

        return new()
        {
            Header = header,
            AnimationGroups = animationGroups,
        };
    }
}