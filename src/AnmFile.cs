using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace WallyAnmSpinzor;

public class AnmFile
{
    public required int Header { get; set; }
    public required Dictionary<string, AnmClass> Classes { get; set; }

    public static AnmFile CreateFrom(Stream stream)
    {
        // needs to be 8 bytes for AnmFrame
        Span<byte> buffer = stackalloc byte[8];

        stream.ReadExactly(buffer[..4]);
        int header = BinaryPrimitives.ReadInt32LittleEndian(buffer[..4]);

        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress);
        return CreateFrom(decompressedStream, header, buffer);
    }

    public void WriteTo(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[8];

        BinaryPrimitives.WriteInt32LittleEndian(buffer[..4], Header);
        stream.Write(buffer[..4]);

        using ZLibStream compressedStream = new(stream, CompressionLevel.SmallestSize);
        WriteTo(compressedStream, buffer);
    }

    internal static AnmFile CreateFrom(Stream stream, int header, Span<byte> buffer)
    {
        Dictionary<string, AnmClass> classes = [];
        stream.ReadExactly(buffer[..1]);
        while (buffer[0] != 0)
        {
            string name = ReadString(stream, buffer);
            AnmClass @class = AnmClass.CreateFrom(stream, buffer);
            classes[name] = @class;

            stream.ReadExactly(buffer[..1]);
        }

        return new()
        {
            Header = header,
            Classes = classes,
        };
    }

    internal void WriteTo(Stream stream, Span<byte> buffer)
    {
        foreach ((string name, AnmClass @class) in Classes)
        {
            stream.WriteByte(1);
            WriteString(stream, buffer, name);
            @class.WriteTo(stream, buffer);
        }
        stream.WriteByte(0);
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