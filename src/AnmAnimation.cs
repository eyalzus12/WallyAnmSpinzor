using System.Collections.Generic;

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

    internal static AnmAnimation CreateFrom(ByteReader br)
    {
        string name = br.ReadFlashString();
        uint frameCount = br.ReadU32LE();
        uint loopStart = br.ReadU32LE();
        uint recoveryStart = br.ReadU32LE();
        uint freeStart = br.ReadU32LE();
        uint previewFrame = br.ReadU32LE();
        uint baseStart = br.ReadU32LE();
        uint dataSize = br.ReadU32LE();
        List<uint> data = [];
        for (int i = 0; i < dataSize; ++i)
            data.Add(br.ReadU32LE());
        // this discarded value tells the game what's the size of the frames field.
        // this is used to be able to load the frames on-demand.
        _ = br.ReadU32LE();
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