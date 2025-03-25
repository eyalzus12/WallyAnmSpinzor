using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

    internal static async Task<AnmClass> CreateFromAsync(Stream stream, Memory<byte> buffer, CancellationToken ctoken = default)
    {
        string index = await stream.GetStrAsync(buffer, ctoken);
        string fileName = await stream.GetStrAsync(buffer, ctoken);

        uint animationCount = await stream.GetU32Async(buffer, ctoken);
        Dictionary<string, AnmAnimation> animations = new((int)animationCount);
        for (int i = 0; i < animationCount; ++i)
        {
            AnmAnimation animation = await AnmAnimation.CreateFromAsync(stream, buffer, ctoken);
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

    internal async Task WriteToAsync(Stream stream, Memory<byte> buffer, CancellationToken ctoken = default)
    {
        await stream.PutStrAsync(Index, buffer, ctoken);
        await stream.PutStrAsync(FileName, buffer, ctoken);
        await stream.PutU32Async((uint)Animations.Count, buffer, ctoken);
        foreach ((_, AnmAnimation animation) in Animations)
        {
            await animation.WriteToAsync(stream, buffer, ctoken);
        }
    }
}