using System;
using System.IO;

namespace WallyAnmSpinzor;

public sealed class AnmFrame
{
    public required short Id { get; set; }
    public required (double X, double Y)? FireSocket { get; set; }
    public required (double X, double Y)? EBPlatformPos { get; set; } // unused
    public required double EBPlatformRot { get; set; } // unused
    public required AnmBone[] Bones { get; set; }

    internal static AnmFrame CreateFrom(Stream stream, AnmFrame? prev)
    {
        short id = stream.GetI16();

        (double x, double y)? fireSocket = null;
        if (stream.GetB())
        {
            double x = stream.GetF64();
            double y = stream.GetF64();
            fireSocket = (x, y);
        }

        (double x, double y)? ebPlatformPos = null;
        if (stream.GetB())
        {
            double x = stream.GetF64();
            double y = stream.GetF64();
            ebPlatformPos = (x, y);
        }
        double ebPlatformRot = stream.GetF64();

        short bonesCount = stream.GetI16();
        AnmBone[] bones = new AnmBone[bonesCount];
        for (int i = 0; i < bonesCount; ++i)
        {
            if (stream.GetB())
            {
                if (prev is null)
                    throw new Exception("Bone duplication in first animation frame");
                if (i >= prev.Bones.Length)
                    throw new Exception("Bone duplication without matching bone in previous frame");
                bones[i] = prev.Bones[i].Clone();
                if (!stream.GetB())
                    bones[i].Frame = stream.GetI16();
            }
            else
            {
                bones[i] = AnmBone.CreateFrom(stream);
            }
        }

        return new()
        {
            Id = id,
            FireSocket = fireSocket,
            EBPlatformPos = ebPlatformPos,
            EBPlatformRot = ebPlatformRot,
            Bones = bones,
        };
    }

    internal void WriteTo(Stream stream, AnmFrame? prevFrame)
    {
        stream.PutI16(Id);

        if (FireSocket is null)
        {
            stream.PutB(false);
        }
        else
        {
            stream.PutB(true);
            stream.PutF64(FireSocket.Value.X);
            stream.PutF64(FireSocket.Value.Y);
        }

        if (EBPlatformPos is null)
        {
            stream.PutB(false);
        }
        else
        {
            stream.PutB(true);
            stream.PutF64(EBPlatformPos.Value.X);
            stream.PutF64(EBPlatformPos.Value.Y);
        }

        stream.PutF64(EBPlatformRot);
        stream.PutI16((short)Bones.Length);
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
                    stream.PutI16(Bones[i].Frame);
                }
            }
            else
            {
                stream.PutB(false);
                Bones[i].WriteTo(stream);
            }
        }
    }

    internal uint GetByteCount(AnmFrame? prevFrame)
    {
        uint size = 0;
        size += sizeof(short); // id
        size += sizeof(byte); // FireSocket indicator
        if (FireSocket is not null)
            size += 2 * sizeof(double); // FireSocket
        size += sizeof(byte); // EB_Platform indicator
        if (EBPlatformPos is not null)
            size += 2 * sizeof(double); // EB_Platform position
        size += sizeof(double); // EB_Platform rotation
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