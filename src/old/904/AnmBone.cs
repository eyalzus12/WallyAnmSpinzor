using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WallyAnmSpinzor.Internal;

namespace WallyAnmSpinzor.Version_904;

public sealed class AnmBone_904
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

    internal static AnmBone_904 CreateFrom(DataReader reader)
    {
        short id = reader.ReadI16();
        bool opaque = reader.ReadBool();
        bool identity = false;
        bool symmetric = false;
        if (reader.ReadBool())
        {
            if (reader.ReadBool()) identity = true;
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
            scaleX = reader.ReadF32();
            rotateSkew0 = reader.ReadF32();
            if (symmetric)
            {
                rotateSkew1 = rotateSkew0;
                scaleY = -scaleX;
            }
            else
            {
                rotateSkew1 = reader.ReadF32();
                scaleY = reader.ReadF32();
            }
        }
        float x = reader.ReadF32();
        float y = reader.ReadF32();
        short frame = reader.ReadI16();
        double opacity = 1.0;
        if (!opaque)
        {
            opacity = reader.ReadU8() / 255.0;
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

    internal static async Task<AnmBone_904> CreateFromAsync(DataReader reader, CancellationToken ctoken = default)
    {
        short id = await reader.ReadI16Async(ctoken);
        bool opaque = await reader.ReadBoolAsync(ctoken);
        bool identity = false;
        bool symmetric = false;
        if (await reader.ReadBoolAsync(ctoken))
        {
            if (await reader.ReadBoolAsync(ctoken)) identity = true;
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
            scaleX = await reader.ReadF32Async(ctoken);
            rotateSkew0 = await reader.ReadF32Async(ctoken);
            if (symmetric)
            {
                rotateSkew1 = rotateSkew0;
                scaleY = -scaleX;
            }
            else
            {
                rotateSkew1 = await reader.ReadF32Async(ctoken);
                scaleY = await reader.ReadF32Async(ctoken);
            }
        }
        float x = await reader.ReadF32Async(ctoken);
        float y = await reader.ReadF32Async(ctoken);
        short frame = await reader.ReadI16Async(ctoken);
        double opacity = 1.0;
        if (!opaque)
        {
            opacity = await reader.ReadU8Async(ctoken) / 255.0;
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

    internal async Task WriteToAsync(Stream stream, CancellationToken ctoken = default)
    {
        await stream.PutI16Async(Id, ctoken);
        await stream.PutBAsync(Opacity == 1, ctoken);

        bool identity = IsIdentity;
        bool symmetric = IsSymmetric;
        if (identity || symmetric)
        {
            await stream.PutBAsync(true, ctoken);
            await stream.PutBAsync(identity, ctoken);
        }
        else
        {
            await stream.PutBAsync(false, ctoken);
        }

        if (!identity)
        {
            await stream.PutF32Async(ScaleX, ctoken);
            await stream.PutF32Async(RotateSkew0, ctoken);
            if (!symmetric)
            {
                await stream.PutF32Async(RotateSkew1, ctoken);
                await stream.PutF32Async(ScaleY, ctoken);
            }
        }
        await stream.PutF32Async(X, ctoken);
        await stream.PutF32Async(Y, ctoken);
        await stream.PutI16Async(Frame, ctoken);
        if (Opacity != 1)
        {
            byte opacity = (byte)Math.Round(Opacity * 255);
            await stream.PutU8Async(opacity, ctoken);
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

    internal AnmBone_904 Clone() => new()
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

    internal bool IsPartialCloneOf(AnmBone_904 bone) =>
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