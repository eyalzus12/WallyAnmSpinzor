using System.IO;

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