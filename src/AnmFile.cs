using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace WallyAnmSpinzor;

public class AnmFile
{
    public required byte[] Header { get; set; }
    public required Dictionary<string, AnmGroup> AnimationGroups { get; set; }

    public static AnmFile CreateFrom(Stream stream)
    {
        byte[] header = new byte[4]; stream.ReadExactly(header, 0, 4);
        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress);
        using BinaryReader br = new(decompressedStream);
        return CreateFrom(br, header);
    }

    internal static AnmFile CreateFrom(BinaryReader br, byte[] header)
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