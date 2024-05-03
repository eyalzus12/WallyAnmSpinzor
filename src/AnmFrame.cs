using System;
using System.Collections.Generic;

namespace WallyAnmSpinzor;

public sealed class AnmFrame
{
    public sealed class Offset
    {
        public required double X { get; set; }
        public required double Y { get; set; }

        internal static Offset? CreateFrom(ByteReader br)
        {
            if (br.ReadU8() == 0) return null;
            double x = br.ReadF64LE();
            double y = br.ReadF64LE();
            return new() { X = x, Y = y };
        }
    }

    public required ushort Id { get; set; }
    public required Offset? OffsetA { get; set; }
    public required Offset? OffsetB { get; set; }
    public required double Rotation { get; set; }
    public required List<AnmBone> Bones { get; set; }

    internal static AnmFrame CreateFrom(ByteReader br, AnmFrame? prev)
    {
        ushort id = br.ReadU16LE();
        Offset? offsetA = Offset.CreateFrom(br);
        Offset? offsetB = Offset.CreateFrom(br);
        double rotation = br.ReadF64LE();
        short bonesCount = br.ReadI16LE();
        List<AnmBone> bones = [];
        for (int i = 0; i < bonesCount; ++i)
        {
            if (br.ReadU8() != 0)
            {
                if (prev is null) throw new Exception("Bone duplication in first animation frame");
                bones.Add(prev.Bones[i].Clone());
                if (br.ReadU8() == 0)
                {
                    bones[i].Frame = br.ReadI16LE();
                }
            }
            else
            {
                bones.Add(AnmBone.CreateFrom(br));
            }
        }

        return new()
        {
            Id = id,
            OffsetA = offsetA,
            OffsetB = offsetB,
            Rotation = rotation,
            Bones = bones,
        };
    }
}