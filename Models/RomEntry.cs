namespace EDGBAPRO_Rom_Manager.Models
{
    public class RomEntry
    {
        public string RomName { get; set; } = "";
        public string SaveType { get; set; } = "UNKNOWN";
        public string RomPath { get; set; } = "";
        public string GameDataFolder { get; set; } = "";
        public string SaveFileName { get; set; } = "";
    }
}
