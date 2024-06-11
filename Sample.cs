using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WallyAnmSpinzor;

internal class Sample
{
    public static void SerializeToJson(string filePath, string outputPath)
    {
        JsonSerializerOptions options = new()
        {
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            WriteIndented = true,
        };

        AnmFile anm;
        using (FileStream file = new(filePath, FileMode.Open, FileAccess.Read))
            anm = AnmFile.CreateFrom(file);
        using (FileStream outFile = new(outputPath, FileMode.Create, FileAccess.Write))
            JsonSerializer.Serialize(outFile, anm, options);
    }

    public static void JsonToAnm(string jsonPath, string outputPath)
    {
        JsonSerializerOptions options = new()
        {
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            WriteIndented = true,
        };
        AnmFile anm;
        using (FileStream file = new(jsonPath, FileMode.Open, FileAccess.Read))
            anm = JsonSerializer.Deserialize<AnmFile>(file, options) ?? throw new Exception("bad json format");
        using (FileStream outFile = new(outputPath, FileMode.Create, FileAccess.Write))
            anm.WriteTo(outFile);
    }
}