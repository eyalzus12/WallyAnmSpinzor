using System;
using System.Buffers.Binary;
using System.IO;

namespace WallyAnmSpinzor;

public sealed class AnmBone
{
    public required ushort Id { get; set; }
    public required float ScaleX { get; set; }
    public required float RotateSkew0 { get; set; }
    public required float RotateSkew1 { get; set; }
    public required float ScaleY { get; set; }
    public required float X { get; set; }
    public required float Y { get; set; }
    public required double Opacity { get; set; }
    public required short Frame { get; set; }

    internal static AnmBone CreateFrom(Stream stream, Span<byte> buffer)
    {
        stream.ReadExactly(buffer[..2]);
        ushort id = BinaryPrimitives.ReadUInt16LittleEndian(buffer[..2]);
        stream.ReadExactly(buffer[..1]);
        bool opaque = buffer[0] != 0;
        bool identity = false;
        bool symmetric = false;
        stream.ReadExactly(buffer[..1]);
        if (buffer[0] != 0)
        {
            stream.ReadExactly(buffer[..1]);
            if (buffer[0] != 0) identity = true;
            else symmetric = true;
        }

        float scaleX, rotateSkew0, rotateSkew1, scaleY;
        if (identity)
        {
            scaleX = scaleY = 1;
            rotateSkew0 = rotateSkew1 = 0;
        }
        else
        {
            stream.ReadExactly(buffer[..4]);
            scaleX = BinaryPrimitives.ReadSingleLittleEndian(buffer[..4]);
            stream.ReadExactly(buffer[..4]);
            rotateSkew0 = BinaryPrimitives.ReadSingleLittleEndian(buffer[..4]);
            if (symmetric)
            {
                rotateSkew1 = rotateSkew0;
                scaleY = -scaleX;
            }
            else
            {
                stream.ReadExactly(buffer[..4]);
                rotateSkew1 = BinaryPrimitives.ReadSingleLittleEndian(buffer[..4]);
                stream.ReadExactly(buffer[..4]);
                scaleY = BinaryPrimitives.ReadSingleLittleEndian(buffer[..4]);
            }
        }
        stream.ReadExactly(buffer[..4]);
        float x = BinaryPrimitives.ReadSingleLittleEndian(buffer[..4]);
        stream.ReadExactly(buffer[..4]);
        float y = BinaryPrimitives.ReadSingleLittleEndian(buffer[..4]);
        stream.ReadExactly(buffer[..2]);
        short frame = BinaryPrimitives.ReadInt16LittleEndian(buffer[..2]);
        double opacity = 1.0;
        if (!opaque)
        {
            stream.ReadExactly(buffer[..1]);
            opacity = buffer[0] / 255.0;
        }

        return new()
        {
            Id = id,
            ScaleX = scaleX,
            RotateSkew0 = rotateSkew0,
            RotateSkew1 = rotateSkew1,
            ScaleY = scaleY,
            X = x,
            Y = y,
            Opacity = opacity,
            Frame = frame,
        };
    }

    internal void WriteTo(Stream stream, Span<byte> buffer)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(buffer[..2], Id);
        stream.Write(buffer[..2]);

        if (Opacity == 1) stream.WriteByte(1);
        else stream.WriteByte(0);

        bool identity = IsIdentity;
        bool symmetric = IsSymmetric;
        if (identity || symmetric)
        {
            stream.WriteByte(1);
            if (identity) stream.WriteByte(1);
            else stream.WriteByte(0);
        }
        else
        {
            stream.WriteByte(0);
        }

        if (!identity)
        {
            BinaryPrimitives.WriteSingleLittleEndian(buffer[..4], ScaleX);
            stream.Write(buffer[..4]);
            BinaryPrimitives.WriteSingleLittleEndian(buffer[..4], RotateSkew0);
            stream.Write(buffer[..4]);
            if (!symmetric)
            {
                BinaryPrimitives.WriteSingleLittleEndian(buffer[..4], RotateSkew1);
                stream.Write(buffer[..4]);
                BinaryPrimitives.WriteSingleLittleEndian(buffer[..4], ScaleY);
                stream.Write(buffer[..4]);
            }
        }
        BinaryPrimitives.WriteSingleLittleEndian(buffer[..4], X);
        stream.Write(buffer[..4]);
        BinaryPrimitives.WriteSingleLittleEndian(buffer[..4], Y);
        stream.Write(buffer[..4]);
        BinaryPrimitives.WriteInt16LittleEndian(buffer[..2], Frame);
        stream.Write(buffer[..2]);
        if (Opacity != 1)
        {
            byte opacity = (byte)Math.Round(Opacity * 255);
            stream.WriteByte(opacity);
        }
    }

    internal uint GetByteCount()
    {
        uint size = 0;
        size += sizeof(ushort); // id
        size += sizeof(byte); // opaque
        size += sizeof(byte); // identity/symmetric indicator
        if (IsIdentity)
        {
            size += sizeof(byte); // 2nd indicator
        }
        else
        {
            size += sizeof(float); // scaleX
            size += sizeof(float); // rotateSkew0
            if (IsSymmetric)
            {
                size += sizeof(byte); // indicator
            }
            else
            {
                size += sizeof(float); // rotateSkew1
                size += sizeof(float); // scaleY
            }
        }
        size += sizeof(float); // x
        size += sizeof(float); // y
        size += sizeof(short); // frame
        if (Opacity != 1)
        {
            size += sizeof(byte); // opacity
        }
        return size;
    }

    internal AnmBone Clone() => new()
    {
        Id = Id,
        ScaleX = ScaleX,
        RotateSkew0 = RotateSkew0,
        RotateSkew1 = RotateSkew1,
        ScaleY = ScaleY,
        X = X,
        Y = Y,
        Opacity = Opacity,
        Frame = Frame,
    };

    internal bool IsPartialCloneOf(AnmBone bone) =>
        Id == bone.Id &&
        ScaleX == bone.ScaleX &&
        RotateSkew0 == bone.RotateSkew0 &&
        RotateSkew1 == bone.RotateSkew1 &&
        ScaleY == bone.ScaleY &&
        X == bone.X &&
        Y == bone.Y &&
        Opacity == bone.Opacity;

    internal bool IsIdentity => ScaleX == 1 && RotateSkew0 == 0 && RotateSkew1 == 0 && ScaleY == 1;
    internal bool IsSymmetric => ScaleY == -ScaleX && RotateSkew0 == RotateSkew1;
}