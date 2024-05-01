using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace WallyAnmSpinzor;

public class AnmFile
{
    public required byte[] Header { get; set; }
    public required Dictionary<string, AnmStore> AnimationStorages { get; set; }

    public static AnmFile CreateFrom(Stream stream)
    {
        byte[] header = new byte[4]; stream.ReadExactly(header, 0, 4);
        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress);
        using BinaryReader br = new(decompressedStream);
        return CreateFrom(br, header);
    }

    internal static AnmFile CreateFrom(BinaryReader br, byte[] header)
    {
        Dictionary<string, AnmStore> animationStorages = [];
        while (br.ReadBoolean())
        {
            string name = br.ReadFlashString();
            AnmStore storage = AnmStore.CreateFrom(br);
            animationStorages[name] = storage;
        }

        return new()
        {
            Header = header,
            AnimationStorages = animationStorages,
        };
    }
}