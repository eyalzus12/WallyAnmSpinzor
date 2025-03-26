using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using WallyAnmSpinzor.Internal;

namespace WallyAnmSpinzor;

public sealed class AnmFile
{
    public required int Header { get; set; }
    public required Dictionary<string, AnmClass> Classes { get; set; }

    public static AnmFile CreateFrom(Stream stream, bool leaveOpen = false)
    {
        int header = ReadHeader(stream);
        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress, leaveOpen);
        using DataReader reader = new(decompressedStream, leaveOpen);
        return CreateFromInternal(reader, header);
    }

    public static async Task<AnmFile> CreateFromAsync(Stream stream, bool leaveOpen = false, CancellationToken ctoken = default)
    {
        int header = await ReadHeaderAsync(stream, ctoken);
        using ZLibStream decompressedStream = new(stream, CompressionMode.Decompress, leaveOpen);
        using DataReader reader = new(decompressedStream, leaveOpen);
        return await CreateFromInternalAsync(reader, header, ctoken);
    }

    public void WriteTo(Stream stream, bool leaveOpen = false)
    {
        WriteHeader(stream);
        using ZLibStream compressedStream = new(stream, CompressionLevel.SmallestSize, leaveOpen);
        using DataWriter writer = new(compressedStream, leaveOpen);
        WriteToInternal(writer);
    }

    public async Task WriteToAsync(Stream stream, bool leaveOpen = false, CancellationToken ctoken = default)
    {
        await WriteHeaderAsync(stream, ctoken);
        using ZLibStream compressedStream = new(stream, CompressionLevel.SmallestSize, leaveOpen);
        using DataWriter writer = new(compressedStream, leaveOpen);
        await WriteToInternalAsync(writer, ctoken);
    }

    internal static int ReadHeader(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt32LittleEndian(buffer);
    }

    internal static async ValueTask<int> ReadHeaderAsync(Stream stream, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[4];
        await stream.ReadExactlyAsync(buffer, ctoken).ConfigureAwait(false);
        return BinaryPrimitives.ReadInt32LittleEndian(buffer);
    }

    internal static AnmFile CreateFromInternal(DataReader reader, int header)
    {
        Dictionary<string, AnmClass> classes = [];
        while (reader.ReadBool())
        {
            string key = reader.ReadStr();
            AnmClass @class = AnmClass.CreateFrom(reader);
            classes[key] = @class;
        }

        return new()
        {
            Header = header,
            Classes = classes,
        };
    }

    internal static async Task<AnmFile> CreateFromInternalAsync(DataReader reader, int header, CancellationToken ctoken = default)
    {
        Dictionary<string, AnmClass> classes = [];
        while (await reader.ReadBoolAsync(ctoken))
        {
            string key = await reader.ReadStrAsync(ctoken);
            AnmClass @class = await AnmClass.CreateFromAsync(reader, ctoken);
            classes[key] = @class;
        }

        return new()
        {
            Header = header,
            Classes = classes,
        };
    }

    internal void WriteHeader(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, Header);
        stream.Write(buffer);
    }

    internal async ValueTask WriteHeaderAsync(Stream stream, CancellationToken ctoken = default)
    {
        byte[] buffer = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, Header);
        await stream.WriteAsync(buffer, ctoken).ConfigureAwait(false);
    }

    internal void WriteToInternal(DataWriter writer)
    {
        foreach ((string key, AnmClass @class) in Classes)
        {
            writer.WriteBool(true);
            writer.WriteStr(key);
            @class.WriteTo(writer);
        }
        writer.WriteBool(false);
    }

    internal async Task WriteToInternalAsync(DataWriter writer, CancellationToken ctoken = default)
    {
        foreach ((string key, AnmClass @class) in Classes)
        {
            await writer.WriteBoolAsync(true, ctoken);
            await writer.WriteStrAsync(key, ctoken);
            await @class.WriteToAsync(writer, ctoken);
        }
        await writer.WriteBoolAsync(false, ctoken);
    }
}