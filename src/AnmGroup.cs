using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WallyAnmSpinzor;

public class AnmGroup
{
    public required string Index { get; set; }
    public required string FileName { get; set; }
    public required List<AnmAnimation> Animations { get; set; }

    internal static AnmGroup CreateFrom(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];

        stream.ReadExactly(buffer[..2]);
        ushort indexLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[..2]);
        Span<byte> indexBuffer = stackalloc byte[indexLength];
        stream.ReadExactly(indexBuffer);
        string index = Encoding.UTF8.GetString(indexBuffer);

        stream.ReadExactly(buffer[..2]);
        ushort fileNameLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[..2]);
        Span<byte> fileNameBuffer = stackalloc byte[fileNameLength];
        stream.ReadExactly(fileNameBuffer);
        string fileName = Encoding.UTF8.GetString(fileNameBuffer);

        stream.ReadExactly(buffer[..4]);
        uint animationCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer[..4]);
        List<AnmAnimation> animations = new((int)animationCount);
        for (int i = 0; i < animationCount; ++i)
            animations.Add(AnmAnimation.CreateFrom(stream));

        return new()
        {
            Index = index,
            FileName = fileName,
            Animations = animations,
        };
    }
}