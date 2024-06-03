using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace WallyAnmSpinzor;

public sealed class AnmFrame
{
    // making this a struct is more memory expensive
    public sealed class Offset
    {
        public required double X { get; set; }
        public required double Y { get; set; }
    }

    public required ushort Id { get; set; }
    public required Offset? OffsetA { get; set; }
    public required Offset? OffsetB { get; set; }
    public required double Rotation { get; set; }
    public required AnmBone[] Bones { get; set; }

    internal static AnmFrame CreateFrom(Stream stream, AnmFrame? prev, Span<byte> buffer)
    {
        stream.ReadExactly(buffer[..2]);
        ushort id = BinaryPrimitives.ReadUInt16LittleEndian(buffer[..2]);

        Offset? offsetA = null;
        stream.ReadExactly(buffer[..1]);
        if (buffer[0] != 0)
        {
            stream.ReadExactly(buffer[..8]);
            double x = BinaryPrimitives.ReadDoubleLittleEndian(buffer[..8]);
            stream.ReadExactly(buffer[..8]);
            double y = BinaryPrimitives.ReadDoubleLittleEndian(buffer[..8]);
            offsetA = new() { X = x, Y = y, };
        }

        Offset? offsetB = null;
        stream.ReadExactly(buffer[..1]);
        if (buffer[0] != 0)
        {
            stream.ReadExactly(buffer[..8]);
            double x = BinaryPrimitives.ReadDoubleLittleEndian(buffer[..8]);
            stream.ReadExactly(buffer[..8]);
            double y = BinaryPrimitives.ReadDoubleLittleEndian(buffer[..8]);
            offsetB = new() { X = x, Y = y, };
        }

        stream.ReadExactly(buffer[..8]);
        double rotation = BinaryPrimitives.ReadDoubleLittleEndian(buffer[..8]);

        stream.ReadExactly(buffer[..2]);
        short bonesCount = BinaryPrimitives.ReadInt16LittleEndian(buffer[..2]);

        AnmBone[] bones = new AnmBone[bonesCount];
        for (int i = 0; i < bonesCount; ++i)
        {
            stream.ReadExactly(buffer[..1]);
            if (buffer[0] != 0)
            {
                if (prev is null)
                    throw new Exception("Bone duplication in first animation frame");
                bones[i] = prev.Bones[i].Clone();
                stream.ReadExactly(buffer[..1]);
                if (buffer[0] == 0)
                {
                    stream.ReadExactly(buffer[..2]);
                    bones[i].Frame = BinaryPrimitives.ReadInt16LittleEndian(buffer[..2]);
                }
            }
            else
            {
                bones[i] = AnmBone.CreateFrom(stream, buffer);
            }
        }

        return new()
        {
            Id = id,
            OffsetA = offsetA,
            OffsetB = offsetB,
            Rotation = rotation,
            Bones = bones,
        };
    }
}