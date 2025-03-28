using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WallyAnmSpinzor.Internal;

namespace WallyAnmSpinzor.Version_904;

public sealed class AnmClass_904
{
    public required string Index { get; set; }
    public required string FileName { get; set; }
    public required Dictionary<string, AnmAnimation_904> Animations { get; set; }

    internal static AnmClass_904 CreateFrom(DataReader reader)
    {
        string index = reader.ReadStr();
        string fileName = reader.ReadStr();

        uint animationCount = reader.ReadU32();
        Dictionary<string, AnmAnimation_904> animations = new((int)animationCount);
        for (uint i = 0; i < animationCount; ++i)
        {
            AnmAnimation_904 animation = AnmAnimation_904.CreateFrom(reader);
            animations[animation.Name] = animation;
        }

        return new()
        {
            Index = index,
            FileName = fileName,
            Animations = animations,
        };
    }

    internal static async Task<AnmClass_904> CreateFromAsync(DataReader reader, CancellationToken ctoken = default)
    {
        string index = await reader.ReadStrAsync(ctoken);
        string fileName = await reader.ReadStrAsync(ctoken);

        uint animationCount = await reader.ReadU32Async(ctoken);
        Dictionary<string, AnmAnimation_904> animations = new((int)animationCount);
        for (uint i = 0; i < animationCount; ++i)
        {
            AnmAnimation_904 animation = await AnmAnimation_904.CreateFromAsync(reader, ctoken);
            animations[animation.Name] = animation;
        }

        return new()
        {
            Index = index,
            FileName = fileName,
            Animations = animations,
        };
    }

    internal void WriteTo(DataWriter writer)
    {
        writer.WriteStr(Index);
        writer.WriteStr(FileName);
        writer.WriteU32((uint)Animations.Count);
        foreach ((_, AnmAnimation_904 animation) in Animations)
        {
            animation.WriteTo(writer);
        }
    }

    internal async Task WriteToAsync(DataWriter writer, CancellationToken ctoken = default)
    {
        await writer.WriteStrAsync(Index, ctoken);
        await writer.WriteStrAsync(FileName, ctoken);
        await writer.WriteU32Async((uint)Animations.Count, ctoken);
        foreach ((_, AnmAnimation_904 animation) in Animations)
        {
            await animation.WriteToAsync(writer, ctoken);
        }
    }
}