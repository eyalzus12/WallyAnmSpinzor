using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WallyAnmSpinzor;

public class AnmAnimation
{
    public required string Name { get; set; }
    public required uint LoopStart { get; set; }
    public required uint RecoveryStart { get; set; }
    public required uint FreeStart { get; set; }
    public required uint PreviewFrame { get; set; }
    public required uint BaseStart { get; set; }
    public required uint[] Data { get; set; }
    public required AnmFrame[] Frames { get; set; }

    internal static AnmAnimation CreateFrom(Stream stream)
    {
        string name = stream.GetStr();
        uint frameCount = stream.GetU32();
        uint loopStart = stream.GetU32();
        uint recoveryStart = stream.GetU32();
        uint freeStart = stream.GetU32();
        uint previewFrame = stream.GetU32();
        uint baseStart = stream.GetU32();
        uint dataSize = stream.GetU32();
        uint[] data = new uint[dataSize];
        for (int i = 0; i < dataSize; ++i)
        {
            data[i] = stream.GetU32();
        }
        // this discarded value tells the game what's the size of the frames field.
        // this is used to be able to load the frames on-demand.
        _ = stream.GetU32();
        AnmFrame[] frames = new AnmFrame[frameCount];
        for (int i = 0; i < frameCount; ++i)
        {
            AnmFrame? prevFrame = i == 0 ? null : frames[i - 1];
            frames[i] = AnmFrame.CreateFrom(stream, prevFrame);
        }

        return new()
        {
            Name = name,
            LoopStart = loopStart,
            RecoveryStart = recoveryStart,
            FreeStart = freeStart,
            PreviewFrame = previewFrame,
            BaseStart = baseStart,
            Data = data,
            Frames = frames,
        };
    }

    internal static async Task<AnmAnimation> CreateFromAsync(Stream stream, CancellationToken ctoken = default)
    {
        string name = await stream.GetStrAsync(ctoken);
        uint frameCount = await stream.GetU32Async(ctoken);
        uint loopStart = await stream.GetU32Async(ctoken);
        uint recoveryStart = await stream.GetU32Async(ctoken);
        uint freeStart = await stream.GetU32Async(ctoken);
        uint previewFrame = await stream.GetU32Async(ctoken);
        uint baseStart = await stream.GetU32Async(ctoken);
        uint dataSize = await stream.GetU32Async(ctoken);
        uint[] data = new uint[dataSize];
        for (int i = 0; i < dataSize; ++i)
        {
            data[i] = await stream.GetU32Async(ctoken);
        }
        // this discarded value tells the game what's the size of the frames field.
        // this is used to be able to load the frames on-demand.
        _ = await stream.GetU32Async(ctoken);
        AnmFrame[] frames = new AnmFrame[frameCount];
        for (int i = 0; i < frameCount; ++i)
        {
            AnmFrame? prevFrame = i == 0 ? null : frames[i - 1];
            frames[i] = await AnmFrame.CreateFromAsync(stream, prevFrame, ctoken);
        }

        return new()
        {
            Name = name,
            LoopStart = loopStart,
            RecoveryStart = recoveryStart,
            FreeStart = freeStart,
            PreviewFrame = previewFrame,
            BaseStart = baseStart,
            Data = data,
            Frames = frames,
        };
    }

    internal void WriteTo(Stream stream)
    {
        stream.PutStr(Name);
        stream.PutU32((uint)Frames.Length);
        stream.PutU32(LoopStart);
        stream.PutU32(RecoveryStart);
        stream.PutU32(FreeStart);
        stream.PutU32(PreviewFrame);
        stream.PutU32(BaseStart);
        stream.PutU32((uint)Data.Length);
        foreach (uint datum in Data)
        {
            stream.PutU32(datum);
        }
        stream.PutU32(GetFramesByteCount());
        for (int i = 0; i < Frames.Length; ++i)
        {
            AnmFrame? prevFrame = i == 0 ? null : Frames[i - 1];
            Frames[i].WriteTo(stream, prevFrame);
        }
    }

    internal async Task WriteToAsync(Stream stream, CancellationToken ctoken = default)
    {
        await stream.PutStrAsync(Name, ctoken);
        await stream.PutU32Async((uint)Frames.Length, ctoken);
        await stream.PutU32Async(LoopStart, ctoken);
        await stream.PutU32Async(RecoveryStart, ctoken);
        await stream.PutU32Async(FreeStart, ctoken);
        await stream.PutU32Async(PreviewFrame, ctoken);
        await stream.PutU32Async(BaseStart, ctoken);
        await stream.PutU32Async((uint)Data.Length, ctoken);
        foreach (uint datum in Data)
        {
            await stream.PutU32Async(datum, ctoken);
        }
        await stream.PutU32Async(GetFramesByteCount(), ctoken);
        for (int i = 0; i < Frames.Length; ++i)
        {
            AnmFrame? prevFrame = i == 0 ? null : Frames[i - 1];
            await Frames[i].WriteToAsync(stream, prevFrame, ctoken);
        }
    }

    internal uint GetFramesByteCount()
    {
        uint size = 0;
        for (int i = 0; i < Frames.Length; ++i)
        {
            AnmFrame? prevFrame = i == 0 ? null : Frames[i - 1];
            size += Frames[i].GetByteCount(prevFrame);
        }
        return size;
    }
}