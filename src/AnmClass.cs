using System.Collections.Generic;
using System.IO;

namespace WallyAnmSpinzor;

public class AnmClass
{
    public required string Index { get; set; }
    public required string FileName { get; set; }
    public required Dictionary<string, AnmAnimation> Animations { get; set; }

    internal static AnmClass CreateFrom(Stream stream)
    {
        string index = stream.GetStr();
        string fileName = stream.GetStr();

        uint animationCount = stream.GetU32();
        Dictionary<string, AnmAnimation> animations = new((int)animationCount);
        for (int i = 0; i < animationCount; ++i)
        {
            AnmAnimation animation = AnmAnimation.CreateFrom(stream);
            animations[animation.Name] = animation;
        }

        return new()
        {
            Index = index,
            FileName = fileName,
            Animations = animations,
        };
    }

    internal void WriteTo(Stream stream)
    {
        stream.PutStr(Index);
        stream.PutStr(FileName);
        stream.PutU32((uint)Animations.Count);
        foreach ((_, AnmAnimation animation) in Animations)
        {
            animation.WriteTo(stream);
        }
    }
}