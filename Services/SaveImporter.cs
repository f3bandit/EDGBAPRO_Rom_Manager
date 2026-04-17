using System.IO;
using EDGBAPRO_Rom_Manager.Models;

namespace EDGBAPRO_Rom_Manager.Services
{
    public static class SaveImporter
    {
        public static void ImportSave(RomEntry entry, string sourceSavePath)
        {
            Directory.CreateDirectory(entry.GameDataFolder);
            File.Copy(sourceSavePath, Path.Combine(entry.GameDataFolder, entry.SaveFileName), true);
        }
    }
}
