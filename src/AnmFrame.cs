using System;
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
        short id = stream.GetI16(buffer);

        Offset? offsetA = null;
        if (stream.GetB())
        {
            double x = stream.GetF64(buffer);
            double y = stream.GetF64(buffer);
            offsetA = new() { X = x, Y = y, };
        }

        Offset? offsetB = null;
        if (stream.GetB())
        {
            double x = stream.GetF64(buffer);
            double y = stream.GetF64(buffer);
            offsetB = new() { X = x, Y = y, };
        }

        double rotation = stream.GetF64(buffer);
        short bonesCount = stream.GetI16(buffer);

        AnmBone[] bones = new AnmBone[bonesCount];
        for (int i = 0; i < bonesCount; ++i)
        {
            if (stream.GetB())
            {
                if (prev is null)
                    throw new Exception("Bone duplication in first animation frame");
                if (prev.Bones.Length >= i)
                    throw new Exception("Bone duplication without matching bone in previous frame");
                bones[i] = prev.Bones[i].Clone();
                if (!stream.GetB())
                    bones[i].Frame = stream.GetI16(buffer);
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
        stream.PutI16(buffer, Id);

        if (OffsetA is null)
        {
            stream.PutB(false);
        }
        else
        {
            stream.PutB(true);
            stream.PutF64(buffer, OffsetA.X);
            stream.PutF64(buffer, OffsetA.Y);
        }

        if (OffsetB is null)
        {
            stream.PutB(false);
        }
        else
        {
            stream.PutB(true);
            stream.PutF64(buffer, OffsetB.X);
            stream.PutF64(buffer, OffsetB.Y);
        }

        stream.PutF64(buffer, Rotation);
        stream.PutI16(buffer, (short)Bones.Length);
        for (int i = 0; i < Bones.Length; ++i)
        {
            if (prevFrame is not null && i < prevFrame.Bones.Length && Bones[i].IsPartialCloneOf(prevFrame.Bones[i]))
            {
                stream.PutB(true);
                if (Bones[i].Frame == prevFrame.Bones[i].Frame)
                {
                    stream.PutB(true);
                }
                else
                {
                    stream.PutB(false);
                    stream.PutI16(buffer, Bones[i].Frame);
                }
            }
            else
            {
                stream.PutB(false);
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