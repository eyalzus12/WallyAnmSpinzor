using System.Collections.Generic;
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
    public required List<uint> Data { get; set; }
    public required List<AnmFrame> Frames { get; set; }

    internal static AnmAnimation CreateFrom(BinaryReader br)
    {
        string name = br.ReadFlashString();
        uint frameCount = br.ReadUInt32();
        uint loopStart = br.ReadUInt32();
        uint recoveryStart = br.ReadUInt32();
        uint freeStart = br.ReadUInt32();
        uint previewFrame = br.ReadUInt32();
        uint baseStart = br.ReadUInt32();
        uint dataSize = br.ReadUInt32();
        List<uint> data = [];
        for (int i = 0; i < dataSize; ++i)
            data.Add(br.ReadUInt32());
        // this discarded value tells the game what's the size of the frames field.
        // this is used to be able to load the frames on-demand.
        _ = br.ReadUInt32();
        List<AnmFrame> frames = [];
        for (int i = 0; i < frameCount; ++i)
            frames.Add(AnmFrame.CreateFrom(br, i == 0 ? null : frames[i - 1]));

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
}