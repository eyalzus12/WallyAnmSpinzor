using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WallyAnmSpinzor.Internal;

namespace WallyAnmSpinzor;

public sealed class AnmFrame
{
    public required short Id { get; set; }
    public required (double X, double Y)? FireSocket { get; set; }
    public required (double X, double Y)? EBPlatformPos { get; set; } // unused
    public required AnmBone[] Bones { get; set; }

    internal static AnmFrame CreateFrom(DataReader reader, AnmFrame? prev)
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

        short bonesCount = reader.ReadI16();
        AnmBone[] bones = new AnmBone[bonesCount];
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
                    bones[i].Frame = reader.ReadI8();
            }
            else
            {
                bones[i] = AnmBone.CreateFrom(reader, i > 0 ? bones[i - 1] : null);
            }
        }

        return new()
        {
            Id = id,
            FireSocket = fireSocket,
            EBPlatformPos = ebPlatformPos,
            Bones = bones,
        };
    }

    internal static async Task<AnmFrame> CreateFromAsync(DataReader reader, AnmFrame? prev, CancellationToken ctoken = default)
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

        short bonesCount = await reader.ReadI16Async(ctoken);
        AnmBone[] bones = new AnmBone[bonesCount];
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
                    bones[i].Frame = await reader.ReadI8Async(ctoken);
            }
            else
            {
                bones[i] = await AnmBone.CreateFromAsync(reader, i > 0 ? bones[i - 1] : null, ctoken);
            }
        }

        return new()
        {
            Id = id,
            FireSocket = fireSocket,
            EBPlatformPos = ebPlatformPos,
            Bones = bones,
        };
    }

    internal void WriteTo(DataWriter writer, AnmFrame? prevFrame)
    {
        writer.WriteI16(Id);

        if (FireSocket is null)
        {
            writer.WriteBool(false);
        }
        else
        {
            writer.WriteBool(true);
            writer.WriteF64(FireSocket.Value.X);
            writer.WriteF64(FireSocket.Value.Y);
        }

        if (EBPlatformPos is null)
        {
            writer.WriteBool(false);
        }
        else
        {
            writer.WriteBool(true);
            writer.WriteF64(EBPlatformPos.Value.X);
            writer.WriteF64(EBPlatformPos.Value.Y);
        }

        writer.WriteI16((short)Bones.Length);
        for (int i = 0; i < Bones.Length; ++i)
        {
            if (prevFrame is not null && i < prevFrame.Bones.Length && Bones[i].IsPartialCloneOf(prevFrame.Bones[i]))
            {
                writer.WriteBool(true);
                if (Bones[i].Frame == prevFrame.Bones[i].Frame)
                {
                    writer.WriteBool(true);
                }
                else
                {
                    writer.WriteBool(false);
                    writer.WriteI8(Bones[i].Frame);
                }
            }
            else
            {
                writer.WriteBool(false);
                Bones[i].WriteTo(writer, i > 0 ? Bones[i - 1] : null);
            }
        }
    }

    internal async Task WriteToAsync(DataWriter writer, AnmFrame? prevFrame, CancellationToken ctoken = default)
    {
        await writer.WriteI16Async(Id, ctoken);

        if (FireSocket is null)
        {
            await writer.WriteBoolAsync(false, ctoken);
        }
        else
        {
            await writer.WriteBoolAsync(true, ctoken);
            await writer.WriteF64Async(FireSocket.Value.X, ctoken);
            await writer.WriteF64Async(FireSocket.Value.Y, ctoken);
        }

        if (EBPlatformPos is null)
        {
            await writer.WriteBoolAsync(false, ctoken);
        }
        else
        {
            await writer.WriteBoolAsync(true, ctoken);
            await writer.WriteF64Async(EBPlatformPos.Value.X, ctoken);
            await writer.WriteF64Async(EBPlatformPos.Value.Y, ctoken);
        }

        await writer.WriteI16Async((short)Bones.Length, ctoken);
        for (int i = 0; i < Bones.Length; ++i)
        {
            if (prevFrame is not null && i < prevFrame.Bones.Length && Bones[i].IsPartialCloneOf(prevFrame.Bones[i]))
            {
                await writer.WriteBoolAsync(true, ctoken);
                if (Bones[i].Frame == prevFrame.Bones[i].Frame)
                {
                    await writer.WriteBoolAsync(true, ctoken);
                }
                else
                {
                    await writer.WriteBoolAsync(false, ctoken);
                    await writer.WriteI8Async(Bones[i].Frame, ctoken);
                }
            }
            else
            {
                await writer.WriteBoolAsync(false, ctoken);
                await Bones[i].WriteToAsync(writer, i > 0 ? Bones[i - 1] : null, ctoken);
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
        size += sizeof(short); // bone count
        for (int i = 0; i < Bones.Length; ++i)
        {
            size += sizeof(byte); // prev frame bone copy indicator
            if (prevFrame is not null && i < prevFrame.Bones.Length && Bones[i].IsPartialCloneOf(prevFrame.Bones[i]))
            {
                size += sizeof(byte); // full copy indicator
                // not a full copy
                if (Bones[i].Frame != prevFrame.Bones[i].Frame)
                    size += sizeof(sbyte); // frame override
            }
            else
            {
                size += Bones[i].GetByteCount(i > 0 ? Bones[i - 1] : null);
            }
        }
        return size;
    }
}