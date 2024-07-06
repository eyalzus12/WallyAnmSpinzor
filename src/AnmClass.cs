using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WallyAnmSpinzor;

public class AnmClass
{
    public required string Index { get; set; }
    public required string FileName { get; set; }
    public required Dictionary<string, AnmAnimation> Animations { get; set; }

    internal static AnmClass CreateFrom(Stream stream, Span<byte> buffer)
    {
        string index = ReadString(stream, buffer);
        string fileName = ReadString(stream, buffer);

        stream.ReadExactly(buffer[..4]);
        uint animationCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer[..4]);
        Dictionary<string, AnmAnimation> animations = new((int)animationCount);
        for (int i = 0; i < animationCount; ++i)
        {
            AnmAnimation animation = AnmAnimation.CreateFrom(stream, buffer);
            animations[animation.Name] = animation;
        }

        return new()
        {
            Index = index,
            FileName = fileName,
            Animations = animations,
        };
    }

    internal void WriteTo(Stream stream, Span<byte> buffer)
    {
        WriteString(stream, buffer, Index);
        WriteString(stream, buffer, FileName);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[..4], (uint)Animations.Count);
        stream.Write(buffer[..4]);
        foreach ((_, AnmAnimation animation) in Animations)
        {
            animation.WriteTo(stream, buffer);
        }
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