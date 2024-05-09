using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

    internal static AnmAnimation CreateFrom(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];

        stream.ReadExactly(buffer[..2]);
        ushort nameLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[..2]);
        Span<byte> nameBuffer = stackalloc byte[nameLength];
        stream.ReadExactly(nameBuffer);
        string name = Encoding.UTF8.GetString(nameBuffer);

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
        List<uint> data = new((int)dataSize);
        for (int i = 0; i < dataSize; ++i)
        {
            stream.ReadExactly(buffer[..4]);
            data.Add(BinaryPrimitives.ReadUInt32LittleEndian(buffer[..4]));
        }
        // this discarded value tells the game what's the size of the frames field.
        // this is used to be able to load the frames on-demand.
        stream.ReadExactly(buffer[..4]);

        List<AnmFrame> frames = new((int)frameCount);
        for (int i = 0; i < frameCount; ++i)
            frames.Add(AnmFrame.CreateFrom(stream, i == 0 ? null : frames[i - 1]));

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