using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WallyAnmSpinzor.Internal;

namespace WallyAnmSpinzor.Version_904;

public sealed class AnmFrame_904
{
    public required short Id { get; set; }
    public required (double X, double Y)? FireSocket { get; set; }
    public required (double X, double Y)? EBPlatformPos { get; set; } // unused
    public required double EBPlatformRot { get; set; } // unused
    public required AnmBone_904[] Bones { get; set; }

    internal static AnmFrame_904 CreateFrom(DataReader reader, AnmFrame_904? prev)
    {
        short id = reader.ReadI16();

        (double x, double y)? fireSocket = null;
        if (reader.ReadBool())
        {
            double x = reader.ReadF64();
            double y = reader.ReadF64();
            fireSocket = (x, y);
        }

        (double x, double y)? ebPlatformPos = null;
        if (reader.ReadBool())
        {
            double x = reader.ReadF64();
            double y = reader.ReadF64();
            ebPlatformPos = (x, y);
        }
        double ebPlatformRot = reader.ReadF64();

        short bonesCount = reader.ReadI16();
        AnmBone_904[] bones = new AnmBone_904[bonesCount];
        for (int i = 0; i < bonesCount; ++i)
        {
            if (reader.ReadBool())
            {
                if (prev is null)
                    throw new Exception("Bone duplication in first animation frame");
                if (i >= prev.Bones.Length)
                    throw new Exception("Bone duplication without matching bone in previous frame");
                bones[i] = prev.Bones[i].Clone();
                if (!reader.ReadBool())
                    bones[i].Frame = reader.ReadI16();
            }
            else
            {
                bones[i] = AnmBone_904.CreateFrom(reader);
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

    internal static async Task<AnmFrame_904> CreateFromAsync(DataReader reader, AnmFrame_904? prev, CancellationToken ctoken = default)
    {
        short id = await reader.ReadI16Async(ctoken);

        (double x, double y)? fireSocket = null;
        if (await reader.ReadBoolAsync(ctoken))
        {
            double x = await reader.ReadF64Async(ctoken);
            double y = await reader.ReadF64Async(ctoken);
            fireSocket = (x, y);
        }

        (double x, double y)? ebPlatformPos = null;
        if (await reader.ReadBoolAsync(ctoken))
        {
            double x = await reader.ReadF64Async(ctoken);
            double y = await reader.ReadF64Async(ctoken);
            ebPlatformPos = (x, y);
        }
        double ebPlatformRot = await reader.ReadF64Async(ctoken);

        short bonesCount = await reader.ReadI16Async(ctoken);
        AnmBone_904[] bones = new AnmBone_904[bonesCount];
        for (int i = 0; i < bonesCount; ++i)
        {
            if (await reader.ReadBoolAsync(ctoken))
            {
                if (prev is null)
                    throw new Exception("Bone duplication in first animation frame");
                if (i >= prev.Bones.Length)
                    throw new Exception("Bone duplication without matching bone in previous frame");
                bones[i] = prev.Bones[i].Clone();
                if (!await reader.ReadBoolAsync(ctoken))
                    bones[i].Frame = await reader.ReadI16Async(ctoken);
            }
            else
            {
                bones[i] = await AnmBone_904.CreateFromAsync(reader, ctoken);
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

    internal void WriteTo(Stream stream, AnmFrame_904? prevFrame)
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

    internal async Task WriteToAsync(Stream stream, AnmFrame_904? prevFrame, CancellationToken ctoken = default)
    {
        await stream.PutI16Async(Id, ctoken);

        if (FireSocket is null)
        {
            await stream.PutBAsync(false, ctoken);
        }
        else
        {
            await stream.PutBAsync(true, ctoken);
            await stream.PutF64Async(FireSocket.Value.X, ctoken);
            await stream.PutF64Async(FireSocket.Value.Y, ctoken);
        }

        if (EBPlatformPos is null)
        {
            await stream.PutBAsync(false, ctoken);
        }
        else
        {
            await stream.PutBAsync(true, ctoken);
            await stream.PutF64Async(EBPlatformPos.Value.X, ctoken);
            await stream.PutF64Async(EBPlatformPos.Value.Y, ctoken);
        }

        await stream.PutF64Async(EBPlatformRot, ctoken);
        await stream.PutI16Async((short)Bones.Length, ctoken);
        for (int i = 0; i < Bones.Length; ++i)
        {
            if (prevFrame is not null && i < prevFrame.Bones.Length && Bones[i].IsPartialCloneOf(prevFrame.Bones[i]))
            {
                await stream.PutBAsync(true, ctoken);
                if (Bones[i].Frame == prevFrame.Bones[i].Frame)
                {
                    await stream.PutBAsync(true, ctoken);
                }
                else
                {
                    await stream.PutBAsync(false, ctoken);
                    await stream.PutI16Async(Bones[i].Frame, ctoken);
                }
            }
            else
            {
                await stream.PutBAsync(false, ctoken);
                await Bones[i].WriteToAsync(stream, ctoken);
            }
        }
    }

    internal uint GetByteCount(AnmFrame_904? prevFrame)
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