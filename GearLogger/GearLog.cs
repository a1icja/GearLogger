using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace GearLogger;

public class GearLog
{
    public required int PieceId { get; set; }
    public required string PieceName { get; set; }
    public required int ItemLevel { get; set; }

    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Intelligence { get; set; }
    public int Mind { get; set; }

    public int CriticalHit { get; set; }
    public int Determination { get; set; }
    public int DirectHit { get; set; }
    public int SkillSpeed { get; set; }
    public int SpellSpeed { get; set; }
    public int Piety { get; set; }
    public int Vitality { get; set; }

    public int Tenacity { get; set; }

    public void LogInfoFormatted()
    {
        Services.Log.Information(
            $"Item: {PieceName} (ID: {PieceId}, ILvl: {ItemLevel})\n" +
            $"  - STR: {Strength}\n" +
            $"  - DEX: {Dexterity}\n" +
            $"  - INT: {Intelligence}\n" +
            $"  - MND: {Mind}\n" +
            $"  - CRT: {CriticalHit}\n" +
            $"  - DET: {Determination}\n" +
            $"  - DHT: {DirectHit}\n" +
            $"  - SKS: {SkillSpeed}\n" +
            $"  - SPS: {SpellSpeed}\n" +
            $"  - PIE: {Piety}\n" +
            $"  - VIT: {Vitality}\n" +
            $"  - TEN: {Tenacity}"
        );
    }

    public static void AppendToCsv(List<GearLog> logs)
    {
        var dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GearLogs");
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var filePath = Path.Combine(dirPath, $"gearlog_{timestamp}.csv");

        var fileExists = File.Exists(filePath);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            // Counter-intuitive: property determines if header _should_ be written.
            HasHeaderRecord = !fileExists,
            Delimiter = ","
        };

        using (var writer = new StreamWriter(filePath, append: fileExists))
        using (var csv = new CsvWriter(writer, config))
        {
            csv.WriteRecords(logs);
        }
    }
}
