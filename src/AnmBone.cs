using System;
using System.IO;

namespace WallyAnmSpinzor;

public sealed class AnmBone
{
    public required short Id { get; set; }
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
        short id = stream.GetI16(buffer);
        bool opaque = stream.GetB();
        bool identity = false;
        bool symmetric = false;
        if (stream.GetB())
        {
            if (stream.GetB()) identity = true;
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
            scaleX = stream.GetF32(buffer);
            rotateSkew0 = stream.GetF32(buffer);
            if (symmetric)
            {
                rotateSkew1 = rotateSkew0;
                scaleY = -scaleX;
            }
            else
            {
                rotateSkew1 = stream.GetF32(buffer);
                scaleY = stream.GetF32(buffer);
            }
        }
        float x = stream.GetF32(buffer);
        float y = stream.GetF32(buffer);
        short frame = stream.GetI16(buffer);
        double opacity = 1.0;
        if (!opaque)
        {
            opacity = stream.GetU8() / 255.0;
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
        stream.PutI16(buffer, Id);
        stream.PutB(Opacity == 1);

        bool identity = IsIdentity;
        bool symmetric = IsSymmetric;
        if (identity || symmetric)
        {
            stream.PutB(true);
            if (identity) stream.PutB(true);
            else stream.PutB(false);
        }
        else
        {
            stream.PutB(false);
        }

        if (!identity)
        {
            stream.PutF32(buffer, ScaleX);
            stream.PutF32(buffer, RotateSkew0);
            if (!symmetric)
            {
                stream.PutF32(buffer, RotateSkew1);
                stream.PutF32(buffer, ScaleY);
            }
        }
        stream.PutF32(buffer, X);
        stream.PutF32(buffer, Y);
        stream.PutI16(buffer, Frame);
        if (Opacity != 1)
        {
            byte opacity = (byte)Math.Round(Opacity * 255);
            stream.PutU8(opacity);
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