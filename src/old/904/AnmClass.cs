using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WallyAnmSpinzor.Version_904;

public class AnmClass_904
{
    public required string Index { get; set; }
    public required string FileName { get; set; }
    public required Dictionary<string, AnmAnimation_904> Animations { get; set; }

    internal static AnmClass_904 CreateFrom(Stream stream)
    {
        string index = stream.GetStr();
        string fileName = stream.GetStr();

        uint animationCount = stream.GetU32();
        Dictionary<string, AnmAnimation_904> animations = new((int)animationCount);
        for (uint i = 0; i < animationCount; ++i)
        {
            AnmAnimation_904 animation = AnmAnimation_904.CreateFrom(stream);
            animations[animation.Name] = animation;
        }

        return new()
        {
            Index = index,
            FileName = fileName,
            Animations = animations,
        };
    }

    internal static async Task<AnmClass_904> CreateFromAsync(Stream stream, CancellationToken ctoken = default)
    {
        string index = await stream.GetStrAsync(ctoken);
        string fileName = await stream.GetStrAsync(ctoken);

        uint animationCount = await stream.GetU32Async(ctoken);
        Dictionary<string, AnmAnimation_904> animations = new((int)animationCount);
        for (uint i = 0; i < animationCount; ++i)
        {
            AnmAnimation_904 animation = await AnmAnimation_904.CreateFromAsync(stream, ctoken);
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
        foreach ((_, AnmAnimation_904 animation) in Animations)
        {
            animation.WriteTo(stream);
        }
    }

    internal async Task WriteToAsync(Stream stream, CancellationToken ctoken = default)
    {
        await stream.PutStrAsync(Index, ctoken);
        await stream.PutStrAsync(FileName, ctoken);
        await stream.PutU32Async((uint)Animations.Count, ctoken);
        foreach ((_, AnmAnimation_904 animation) in Animations)
        {
            await animation.WriteToAsync(stream, ctoken);
        }
    }
}