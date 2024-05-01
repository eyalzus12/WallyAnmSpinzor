using System.Collections.Generic;
using System.IO;

namespace WallyAnmSpinzor;

public class AnmStore
{
    public required string Index { get; set; }
    public required string FileName { get; set; }
    public required List<AnmAnimation> Animations { get; set; }

    internal static AnmStore CreateFrom(BinaryReader br)
    {
        string index = br.ReadFlashString();
        string fileName = br.ReadFlashString();
        uint animationCount = br.ReadUInt32();
        List<AnmAnimation> animations = [];
        for (int i = 0; i < animationCount; ++i)
            animations.Add(AnmAnimation.CreateFrom(br));

        return new()
        {
            Index = index,
            FileName = fileName,
            Animations = animations,
        };
    }
}