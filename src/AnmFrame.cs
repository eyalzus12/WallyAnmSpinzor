using System;
using System.Collections.Generic;
using System.IO;

namespace WallyAnmSpinzor;

public sealed class AnmFrame
{
    public sealed class Offset
    {
        public required double X { get; set; }
        public required double Y { get; set; }

        internal static Offset? CreateFrom(BinaryReader br)
        {
            if (!br.ReadBoolean()) return null;
            double x = br.ReadDouble();
            double y = br.ReadDouble();
            return new() { X = x, Y = y };
        }
    }

    public required ushort Id { get; set; }
    public required Offset? OffsetA { get; set; }
    public required Offset? OffsetB { get; set; }
    public required double Rotation { get; set; }
    public required List<AnmBone> Bones { get; set; }

    internal static AnmFrame CreateFrom(BinaryReader br, AnmFrame? prev)
    {
        ushort id = br.ReadUInt16();
        Offset? offsetA = Offset.CreateFrom(br);
        Offset? offsetB = Offset.CreateFrom(br);
        double rotation = br.ReadDouble();
        short bonesCount = br.ReadInt16();
        List<AnmBone> bones = [];
        for (int i = 0; i < bonesCount; ++i)
        {
            if (br.ReadBoolean())
            {
                if (prev is null) throw new Exception("Bone duplication in first animation frame");
                bones.Add(prev.Bones[i].Clone());
                if (!br.ReadBoolean())
                {
                    bones[i].Frame = br.ReadInt16();
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