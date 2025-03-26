using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WallyAnmSpinzor.Internal;

namespace WallyAnmSpinzor;

public sealed class AnmAnimation
{
    public required string Name { get; set; }
    public required uint LoopStart { get; set; }
    public required uint RecoveryStart { get; set; }
    public required uint FreeStart { get; set; }
    public required uint PreviewFrame { get; set; }
    public required uint BaseStart { get; set; }
    public required uint[] Data { get; set; }
    public required AnmFrame[] Frames { get; set; }

    internal static AnmAnimation CreateFrom(DataReader reader)
    {
        string name = reader.ReadStr();
        uint frameCount = reader.ReadU32();
        uint loopStart = reader.ReadU32();
        uint recoveryStart = reader.ReadU32();
        uint freeStart = reader.ReadU32();
        uint previewFrame = reader.ReadU32();
        uint baseStart = reader.ReadU32();
        uint dataSize = reader.ReadU32();
        uint[] data = new uint[dataSize];
        for (int i = 0; i < dataSize; ++i)
        {
            data[i] = reader.ReadU32();
        }
        // this discarded value tells the game what's the size of the frames field.
        // this is used to be able to load the frames on-demand.
        _ = reader.ReadU32();
        AnmFrame[] frames = new AnmFrame[frameCount];
        for (int i = 0; i < frameCount; ++i)
        {
            AnmFrame? prevFrame = i == 0 ? null : frames[i - 1];
            frames[i] = AnmFrame.CreateFrom(reader, prevFrame);
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

    internal static async Task<AnmAnimation> CreateFromAsync(DataReader reader, CancellationToken ctoken = default)
    {
        string name = await reader.ReadStrAsync(ctoken);
        uint frameCount = await reader.ReadU32Async(ctoken);
        uint loopStart = await reader.ReadU32Async(ctoken);
        uint recoveryStart = await reader.ReadU32Async(ctoken);
        uint freeStart = await reader.ReadU32Async(ctoken);
        uint previewFrame = await reader.ReadU32Async(ctoken);
        uint baseStart = await reader.ReadU32Async(ctoken);
        uint dataSize = await reader.ReadU32Async(ctoken);
        uint[] data = new uint[dataSize];
        for (int i = 0; i < dataSize; ++i)
        {
            data[i] = await reader.ReadU32Async(ctoken);
        }
        // this discarded value tells the game what's the size of the frames field.
        // this is used to be able to load the frames on-demand.
        _ = await reader.ReadU32Async(ctoken);
        AnmFrame[] frames = new AnmFrame[frameCount];
        for (int i = 0; i < frameCount; ++i)
        {
            AnmFrame? prevFrame = i == 0 ? null : frames[i - 1];
            frames[i] = await AnmFrame.CreateFromAsync(reader, prevFrame, ctoken);
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