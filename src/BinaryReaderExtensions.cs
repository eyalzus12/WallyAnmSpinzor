using System.IO;
using System.Text;

namespace WallyAnmSpinzor;

public static class BinaryReaderExtensions
{
    internal static string ReadFlashString(this BinaryReader br)
    {
        ushort length = br.ReadUInt16();
        byte[] bytes = br.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }
}