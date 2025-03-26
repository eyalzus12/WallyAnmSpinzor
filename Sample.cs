using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WallyAnmSpinzor;

internal class Sample
{
    public static async Task SerializeToJson(string filePath, string outputPath)
    {
        JsonSerializerOptions options = new()
        {
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            WriteIndented = true,
        };

        AnmFile anm;
        using (FileStream file = new(filePath, FileMode.Open, FileAccess.Read))
            anm = await AnmFile.CreateFromAsync(file);
        using (FileStream outFile = new(outputPath, FileMode.Create, FileAccess.Write))
            await JsonSerializer.SerializeAsync(outFile, anm, options);
    }

    public static async Task JsonToAnm(string jsonPath, string outputPath)
    {
        JsonSerializerOptions options = new()
        {
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            WriteIndented = true,
        };
        AnmFile anm;
        using (FileStream file = new(jsonPath, FileMode.Open, FileAccess.Read))
            anm = await JsonSerializer.DeserializeAsync<AnmFile>(file, options) ?? throw new Exception("bad json format");
        using (FileStream outFile = new(outputPath, FileMode.Create, FileAccess.Write))
            await anm.WriteToAsync(outFile);
    }
}