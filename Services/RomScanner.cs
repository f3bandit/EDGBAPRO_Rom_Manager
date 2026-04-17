using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EDGBAPRO_Rom_Manager.Models;

namespace EDGBAPRO_Rom_Manager.Services
{
    public class ScanProgressInfo
    {
        public int Percent { get; set; }
        public string Activity { get; set; } = "";
    }

    public static class RomScanner
    {
        public static async Task<List<RomEntry>> ScanAsync(
            string rootFolder,
            IProgress<ScanProgressInfo>? progress = null)
        {
            return await Task.Run(() =>
            {
                var results = new List<RomEntry>();

                var romFiles = Directory.EnumerateFiles(rootFolder, "*.gba", SearchOption.AllDirectories)
                    .Where(path => !IsInsideGameData(path, rootFolder))
                    .Where(path => !ShouldIgnoreFile(path))
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                string gamedataRoot = Path.Combine(rootFolder, "edgba", "gamedata");
                Directory.CreateDirectory(gamedataRoot);

                int total = romFiles.Count;
                if (total == 0)
                {
                    DatabaseWriter.WriteAll(rootFolder, results);
                    progress?.Report(new ScanProgressInfo { Percent = 100, Activity = "No GBA ROMs found." });
                    return results;
                }

                for (int i = 0; i < total; i++)
                {
                    string romPath = romFiles[i];
                    string romName = Path.GetFileNameWithoutExtension(romPath);
                    int percent = (int)Math.Round(((i + 1) / (double)total) * 100.0);

                    progress?.Report(new ScanProgressInfo
                    {
                        Percent = percent,
                        Activity = $"Reading file: {Path.GetFileName(romPath)}"
                    });

                    string saveType = SaveTypeDetector.Detect(romPath);
                    string romFolderName = romName + ".gba";
                    string gameFolder = Path.Combine(gamedataRoot, romFolderName);
                    Directory.CreateDirectory(gameFolder);

                    progress?.Report(new ScanProgressInfo
                    {
                        Percent = percent,
                        Activity = $"Updating folder: {gameFolder}"
                    });

                    string saveFileName = SaveTypeDetector.GetBramFileName(saveType);
                    string saveFilePath = Path.Combine(gameFolder, saveFileName);
                    if (!File.Exists(saveFilePath))
                    {
                        using (var fs = File.Create(saveFilePath))
                        {
                        }
                    }

                    results.Add(new RomEntry
                    {
                        RomName = romName,
                        SaveType = saveType,
                        RomPath = romPath,
                        GameDataFolder = gameFolder,
                        SaveFileName = saveFileName
                    });
                }

                DatabaseWriter.WriteAll(rootFolder, results);
                progress?.Report(new ScanProgressInfo { Percent = 100, Activity = "Scan complete." });
                return results;
            });
        }

        private static bool ShouldIgnoreFile(string path)
        {
            string name = Path.GetFileName(path).ToLowerInvariant();
            return name == "goomba.gba" || name == "pocketnes.gba";
        }

        private static bool IsInsideGameData(string path, string rootFolder)
        {
            string normalized = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar).ToLowerInvariant();
            string gamedata = Path.GetFullPath(Path.Combine(rootFolder, "edgba", "gamedata"))
                .TrimEnd(Path.DirectorySeparatorChar)
                .ToLowerInvariant();

            return normalized.StartsWith(gamedata + Path.DirectorySeparatorChar);
        }
    }
}
