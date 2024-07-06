using System;
using System.Buffers.Binary;
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

    public required short Id { get; set; }
    public required Offset? OffsetA { get; set; }
    public required Offset? OffsetB { get; set; }
    public required double Rotation { get; set; }
    public required AnmBone[] Bones { get; set; }

    internal static AnmFrame CreateFrom(Stream stream, AnmFrame? prev, Span<byte> buffer)
    {
        stream.ReadExactly(buffer[..2]);
        short id = BinaryPrimitives.ReadInt16LittleEndian(buffer[..2]);

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

    internal void WriteTo(Stream stream, Span<byte> buffer, AnmFrame? prevFrame)
    {
        BinaryPrimitives.WriteInt16LittleEndian(buffer[..2], Id);
        stream.Write(buffer[..2]);

        if (OffsetA is null)
        {
            stream.WriteByte(0);
        }
        else
        {
            stream.WriteByte(1);
            BinaryPrimitives.WriteDoubleLittleEndian(buffer[..8], OffsetA.X);
            stream.Write(buffer[..8]);
            BinaryPrimitives.WriteDoubleLittleEndian(buffer[..8], OffsetA.Y);
            stream.Write(buffer[..8]);
        }

        if (OffsetB is null)
        {
            stream.WriteByte(0);
        }
        else
        {
            stream.WriteByte(1);
            BinaryPrimitives.WriteDoubleLittleEndian(buffer[..8], OffsetB.X);
            stream.Write(buffer[..8]);
            BinaryPrimitives.WriteDoubleLittleEndian(buffer[..8], OffsetB.Y);
            stream.Write(buffer[..8]);
        }

        BinaryPrimitives.WriteDoubleLittleEndian(buffer[..8], Rotation);
        stream.Write(buffer[..8]);
        BinaryPrimitives.WriteInt16LittleEndian(buffer[..2], (short)Bones.Length);
        stream.Write(buffer[..2]);
        for (int i = 0; i < Bones.Length; ++i)
        {
            if (prevFrame is not null && i < prevFrame.Bones.Length && Bones[i].IsPartialCloneOf(prevFrame.Bones[i]))
            {
                stream.WriteByte(1);
                if (Bones[i].Frame == prevFrame.Bones[i].Frame)
                {
                    stream.WriteByte(1);
                }
                else
                {
                    stream.WriteByte(0);
                    BinaryPrimitives.WriteInt16LittleEndian(buffer[..2], Bones[i].Frame);
                    stream.Write(buffer[..2]);
                }
            }
            else
            {
                stream.WriteByte(0);
                Bones[i].WriteTo(stream, buffer);
            }
        }
    }

    internal uint GetByteCount(AnmFrame? prevFrame)
    {
        uint size = 0;
        size += sizeof(short); // id
        size += sizeof(byte); // OffsetA indicator
        if (OffsetA is not null)
            size += 2 * sizeof(double); // OffsetA
        size += sizeof(byte); // OffsetB indicator
        if (OffsetB is not null)
            size += 2 * sizeof(double); // OffsetB
        size += sizeof(double); // Rotation
        size += sizeof(short); // bone count
        for (int i = 0; i < Bones.Length; ++i)
        {
            size += sizeof(byte); // prev frame bone copy indicator
            if (prevFrame is not null && i < prevFrame.Bones.Length && Bones[i].IsPartialCloneOf(prevFrame.Bones[i]))
            {
                size += sizeof(byte); // full copy indicator
                // not a full copy
                if (Bones[i].Frame != prevFrame.Bones[i].Frame)
                    size += sizeof(short); // frame override
            }
            else
            {
                size += Bones[i].GetByteCount();
            }
        }
        return size;
    }
}