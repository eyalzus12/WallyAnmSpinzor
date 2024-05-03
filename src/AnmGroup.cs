using System.Collections.Generic;

namespace WallyAnmSpinzor;

public class AnmGroup
{
    public required string Index { get; set; }
    public required string FileName { get; set; }
    public required List<AnmAnimation> Animations { get; set; }

    internal static AnmGroup CreateFrom(ByteReader br)
    {
        string index = br.ReadFlashString();
        string fileName = br.ReadFlashString();
        uint animationCount = br.ReadU32LE();
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