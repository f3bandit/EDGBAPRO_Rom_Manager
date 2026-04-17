using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using EDGBAPRO_Rom_Manager.Models;

namespace EDGBAPRO_Rom_Manager.Services
{
    public static class DatabaseWriter
    {
        public static void WriteAll(string rootFolder, IEnumerable<RomEntry> entries)
        {
            string csvPath = Path.Combine(rootFolder, "ROMS_DB.csv");
            string jsonPath = Path.Combine(rootFolder, "ROMS_DB.json");

            var ordered = entries
                .OrderBy(e => e.RomName, System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("rom_name,save_type,rom_path,game_data_folder,save_file_name");

            foreach (var e in ordered)
            {
                sb.AppendLine(string.Join(",",
                    Escape(e.RomName),
                    Escape(e.SaveType),
                    Escape(e.RomPath),
                    Escape(e.GameDataFolder),
                    Escape(e.SaveFileName)));
            }

            File.WriteAllText(csvPath, sb.ToString(), Encoding.UTF8);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(ordered, options), Encoding.UTF8);
        }

        private static string Escape(string value)
        {
            value ??= string.Empty;

            if (value.Contains("\""))
            {
                value = value.Replace("\"", "\"\"");
            }

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return "\"" + value + "\"";
            }

            return value;
        }
    }
}
