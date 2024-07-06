using System;
using System.Collections.Generic;
using System.IO;

namespace WallyAnmSpinzor;

public class AnmClass
{
    public required string Index { get; set; }
    public required string FileName { get; set; }
    public required Dictionary<string, AnmAnimation> Animations { get; set; }

    internal static AnmClass CreateFrom(Stream stream, Span<byte> buffer)
    {
        string index = stream.GetStr(buffer);
        string fileName = stream.GetStr(buffer);

        uint animationCount = stream.GetU32(buffer);
        Dictionary<string, AnmAnimation> animations = new((int)animationCount);
        for (int i = 0; i < animationCount; ++i)
        {
            AnmAnimation animation = AnmAnimation.CreateFrom(stream, buffer);
            animations[animation.Name] = animation;
        }

        return new()
        {
            Index = index,
            FileName = fileName,
            Animations = animations,
        };
    }

    internal void WriteTo(Stream stream, Span<byte> buffer)
    {
        stream.PutStr(buffer, Index);
        stream.PutStr(buffer, FileName);
        stream.PutU32(buffer, (uint)Animations.Count);
        foreach ((_, AnmAnimation animation) in Animations)
        {
            animation.WriteTo(stream, buffer);
        }
    }
}