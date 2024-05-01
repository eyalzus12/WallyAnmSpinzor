using System;
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
        // why isn't this simpler to do
        byte[] headerBytes = new byte[4];
        stream.ReadExactly(headerBytes, 0, 4);
        if (!BitConverter.IsLittleEndian) Array.Reverse(headerBytes);
        int header = BitConverter.ToInt32(headerBytes);

        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress);
        using BinaryReader br = new(decompressedStream);
        return CreateFrom(br, header);
    }

    internal static AnmFile CreateFrom(BinaryReader br, int header)
    {
        Dictionary<string, AnmGroup> animationGroups = [];
        while (br.ReadBoolean())
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