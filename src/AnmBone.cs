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

    internal static AnmBone CreateFrom(BinaryReader br)
    {
        ushort id = br.ReadUInt16();
        bool opaque = br.ReadBoolean();
        bool identity = false;
        bool symmetric = false;
        if (br.ReadBoolean())
        {
            if (br.ReadBoolean()) identity = true;
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
            scaleX = br.ReadSingle();
            rotateSkew0 = br.ReadSingle();
            if (symmetric)
            {
                rotateSkew1 = rotateSkew0;
                scaleY = -scaleX;
            }
            else
            {
                rotateSkew1 = br.ReadSingle();
                scaleY = br.ReadSingle();
            }
        }
        float x = br.ReadSingle();
        float y = br.ReadSingle();
        short frame = br.ReadInt16();
        double opacity = 1.0;
        if (!opaque)
        {
            opacity = br.ReadByte() / 255.0;
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

    internal AnmBone Clone() => new()
    {
        Id = this.Id,
        ScaleX = this.ScaleX,
        RotateSkew0 = this.RotateSkew0,
        RotateSkew1 = this.RotateSkew1,
        ScaleY = this.ScaleY,
        X = this.X,
        Y = this.Y,
        Opacity = this.Opacity,
        Frame = this.Frame,
    };
}