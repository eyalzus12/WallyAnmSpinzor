using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

    internal static AnmBone CreateFrom(Stream stream)
    {
        short id = stream.GetI16();
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
            scaleX = stream.GetF32();
            rotateSkew0 = stream.GetF32();
            if (symmetric)
            {
                rotateSkew1 = rotateSkew0;
                scaleY = -scaleX;
            }
            else
            {
                rotateSkew1 = stream.GetF32();
                scaleY = stream.GetF32();
            }
        }
        float x = stream.GetF32();
        float y = stream.GetF32();
        short frame = stream.GetI16();
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

    internal static async Task<AnmBone> CreateFromAsync(Stream stream, Memory<byte> buffer, CancellationToken ctoken = default)
    {
        short id = await stream.GetI16Async(buffer, ctoken);
        bool opaque = await stream.GetBAsync(buffer, ctoken);
        bool identity = false;
        bool symmetric = false;
        if (await stream.GetBAsync(buffer, ctoken))
        {
            if (await stream.GetBAsync(buffer, ctoken)) identity = true;
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
            scaleX = await stream.GetF32Async(buffer, ctoken);
            rotateSkew0 = await stream.GetF32Async(buffer, ctoken);
            if (symmetric)
            {
                rotateSkew1 = rotateSkew0;
                scaleY = -scaleX;
            }
            else
            {
                rotateSkew1 = await stream.GetF32Async(buffer, ctoken);
                scaleY = await stream.GetF32Async(buffer, ctoken);
            }
        }
        float x = await stream.GetF32Async(buffer, ctoken);
        float y = await stream.GetF32Async(buffer, ctoken);
        short frame = await stream.GetI16Async(buffer, ctoken);
        double opacity = 1.0;
        if (!opaque)
        {
            opacity = await stream.GetU8Async(buffer, ctoken) / 255.0;
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

    internal void WriteTo(Stream stream)
    {
        stream.PutI16(Id);
        stream.PutB(Opacity == 1);

        bool identity = IsIdentity;
        bool symmetric = IsSymmetric;
        if (identity || symmetric)
        {
            stream.PutB(true);
            stream.PutB(identity);
        }
        else
        {
            stream.PutB(false);
        }

        if (!identity)
        {
            stream.PutF32(ScaleX);
            stream.PutF32(RotateSkew0);
            if (!symmetric)
            {
                stream.PutF32(RotateSkew1);
                stream.PutF32(ScaleY);
            }
        }
        stream.PutF32(X);
        stream.PutF32(Y);
        stream.PutI16(Frame);
        if (Opacity != 1)
        {
            byte opacity = (byte)Math.Round(Opacity * 255);
            stream.PutU8(opacity);
        }
    }

    internal async Task WriteToAsync(Stream stream, Memory<byte> buffer, CancellationToken ctoken = default)
    {
        await stream.PutI16Async(Id, buffer, ctoken);
        await stream.PutBAsync(Opacity == 1, buffer, ctoken);

        bool identity = IsIdentity;
        bool symmetric = IsSymmetric;
        if (identity || symmetric)
        {
            await stream.PutBAsync(true, buffer, ctoken);
            await stream.PutBAsync(identity, buffer, ctoken);
        }
        else
        {
            await stream.PutBAsync(false, buffer, ctoken);
        }

        if (!identity)
        {
            await stream.PutF32Async(ScaleX, buffer, ctoken);
            await stream.PutF32Async(RotateSkew0, buffer, ctoken);
            if (!symmetric)
            {
                await stream.PutF32Async(RotateSkew1, buffer, ctoken);
                await stream.PutF32Async(ScaleY, buffer, ctoken);
            }
        }
        await stream.PutF32Async(X, buffer, ctoken);
        await stream.PutF32Async(Y, buffer, ctoken);
        await stream.PutI16Async(Frame, buffer, ctoken);
        if (Opacity != 1)
        {
            byte opacity = (byte)Math.Round(Opacity * 255);
            await stream.PutU8Async(opacity, buffer, ctoken);
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