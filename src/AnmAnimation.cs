using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace WallyAnmSpinzor;

public class AnmAnimation
{
    public required uint LoopStart { get; set; }
    public required uint RecoveryStart { get; set; }
    public required uint FreeStart { get; set; }
    public required uint PreviewFrame { get; set; }
    public required uint BaseStart { get; set; }
    public required uint[] Data { get; set; }
    public required AnmFrame[] Frames { get; set; }

    internal static AnmAnimation CreateFrom(Stream stream, Span<byte> buffer)
    {
        stream.ReadExactly(buffer[..4]);
        uint frameCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer[..4]);
        stream.ReadExactly(buffer[..4]);
        uint loopStart = BinaryPrimitives.ReadUInt32LittleEndian(buffer[..4]);
        stream.ReadExactly(buffer[..4]);
        uint recoveryStart = BinaryPrimitives.ReadUInt32LittleEndian(buffer[..4]);
        stream.ReadExactly(buffer[..4]);
        uint freeStart = BinaryPrimitives.ReadUInt32LittleEndian(buffer[..4]);
        stream.ReadExactly(buffer[..4]);
        uint previewFrame = BinaryPrimitives.ReadUInt32LittleEndian(buffer[..4]);
        stream.ReadExactly(buffer[..4]);
        uint baseStart = BinaryPrimitives.ReadUInt32LittleEndian(buffer[..4]);
        stream.ReadExactly(buffer[..4]);
        uint dataSize = BinaryPrimitives.ReadUInt32LittleEndian(buffer[..4]);
        uint[] data = new uint[dataSize];
        for (int i = 0; i < dataSize; ++i)
        {
            stream.ReadExactly(buffer[..4]);
            data[i] = BinaryPrimitives.ReadUInt32LittleEndian(buffer[..4]);
        }
        // this discarded value tells the game what's the size of the frames field.
        // this is used to be able to load the frames on-demand.
        stream.ReadExactly(buffer[..4]);

        AnmFrame[] frames = new AnmFrame[frameCount];
        for (int i = 0; i < frameCount; ++i)
        {
            AnmFrame? prevFrame = i == 0 ? null : frames[i - 1];
            frames[i] = AnmFrame.CreateFrom(stream, prevFrame, buffer);
        }

        return new()
        {
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