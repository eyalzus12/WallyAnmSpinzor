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

    internal void WriteTo(DataWriter writer)
    {
        writer.WriteStr(Name);
        writer.WriteU32((uint)Frames.Length);
        writer.WriteU32(LoopStart);
        writer.WriteU32(RecoveryStart);
        writer.WriteU32(FreeStart);
        writer.WriteU32(PreviewFrame);
        writer.WriteU32(BaseStart);
        writer.WriteU32((uint)Data.Length);
        foreach (uint datum in Data)
        {
            writer.WriteU32(datum);
        }
        writer.WriteU32(GetFramesByteCount());
        for (int i = 0; i < Frames.Length; ++i)
        {
            AnmFrame? prevFrame = i == 0 ? null : Frames[i - 1];
            Frames[i].WriteTo(writer, prevFrame);
        }
    }

    internal async Task WriteToAsync(DataWriter writer, CancellationToken ctoken = default)
    {
        await writer.WriteStrAsync(Name, ctoken);
        await writer.WriteU32Async((uint)Frames.Length, ctoken);
        await writer.WriteU32Async(LoopStart, ctoken);
        await writer.WriteU32Async(RecoveryStart, ctoken);
        await writer.WriteU32Async(FreeStart, ctoken);
        await writer.WriteU32Async(PreviewFrame, ctoken);
        await writer.WriteU32Async(BaseStart, ctoken);
        await writer.WriteU32Async((uint)Data.Length, ctoken);
        foreach (uint datum in Data)
        {
            await writer.WriteU32Async(datum, ctoken);
        }
        await writer.WriteU32Async(GetFramesByteCount(), ctoken);
        for (int i = 0; i < Frames.Length; ++i)
        {
            AnmFrame? prevFrame = i == 0 ? null : Frames[i - 1];
            await Frames[i].WriteToAsync(writer, prevFrame, ctoken);
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