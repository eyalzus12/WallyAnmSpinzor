using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WallyAnmSpinzor;

public class AnmClass
{
    public required string Index { get; set; }
    public required string FileName { get; set; }
    public required Dictionary<string, AnmAnimation> Animations { get; set; }

    internal static AnmClass CreateFrom(Stream stream, Span<byte> buffer)
    {
        string index = ReadString(stream, buffer);
        string fileName = ReadString(stream, buffer);

        stream.ReadExactly(buffer[..4]);
        uint animationCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer[..4]);
        Dictionary<string, AnmAnimation> animations = new((int)animationCount);
        for (int i = 0; i < animationCount; ++i)
        {
            string name = ReadString(stream, buffer);
            AnmAnimation animation = AnmAnimation.CreateFrom(stream, buffer);
            animations[name] = animation;
        }

        return new()
        {
            Index = index,
            FileName = fileName,
            Animations = animations,
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