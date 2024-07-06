using System;
using System.Buffers.Binary;
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
    public required uint[] Data { get; set; }
    public required AnmFrame[] Frames { get; set; }

    internal static AnmAnimation CreateFrom(Stream stream, Span<byte> buffer)
    {
        string name = ReadString(stream, buffer);
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

    internal void WriteTo(Stream stream, Span<byte> buffer)
    {
        WriteString(stream, buffer, Name);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[..4], (uint)Frames.Length);
        stream.Write(buffer[..4]);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[..4], LoopStart);
        stream.Write(buffer[..4]);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[..4], RecoveryStart);
        stream.Write(buffer[..4]);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[..4], FreeStart);
        stream.Write(buffer[..4]);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[..4], PreviewFrame);
        stream.Write(buffer[..4]);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[..4], BaseStart);
        stream.Write(buffer[..4]);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[..4], (uint)Data.Length);
        stream.Write(buffer[..4]);
        foreach (uint datum in Data)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer[..4], datum);
            stream.Write(buffer[..4]);
        }
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[..4], GetFramesByteCount());
        stream.Write(buffer[..4]);
        for (int i = 0; i < Frames.Length; ++i)
        {
            AnmFrame? prevFrame = i == 0 ? null : Frames[i - 1];
            Frames[i].WriteTo(stream, buffer, prevFrame);
        }
    }

    internal uint GetFramesByteCount()
    {
        uint size = 0;
        for (int i = 0; i < Frames.Length; ++i)
        {
            AnmFrame? prevFrame = i == 0 ? null : Frames[i - 1];
            size += Frames[i].GetByteCount(prevFrame);
        }
        return size;
    }

    private static string ReadString(Stream stream, Span<byte> buffer)
    {
        stream.ReadExactly(buffer[..2]);
        ushort stringLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[..2]);
        Span<byte> stringBuffer = stackalloc byte[stringLength];
        stream.ReadExactly(stringBuffer);
        return Encoding.UTF8.GetString(stringBuffer);
    }

    private static void WriteString(Stream stream, Span<byte> buffer, string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        ushort len = (ushort)bytes.Length;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer[..2], len);
        stream.Write(buffer[..2]);
        stream.Write(bytes);
    }
}