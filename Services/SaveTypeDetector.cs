using System.IO;
using System.Text;

namespace EDGBAPRO_Rom_Manager.Services
{
    public static class SaveTypeDetector
    {
        public static string Detect(string romPath)
        {
            byte[] data = File.ReadAllBytes(romPath);
            string text = Encoding.ASCII.GetString(data);

            if (text.Contains("EEPROM_V"))
                return "EEPROM";

            if (text.Contains("SRAM_V") || text.Contains("SRAM_F_V"))
                return "SRAM";

            if (text.Contains("FLASH_V") || text.Contains("FLASH512_V") || text.Contains("FLASH1M_V"))
                return "FLASH";

            return "SRAM";
        }

        public static string GetBramFileName(string saveType)
        {
            switch (saveType.ToUpperInvariant())
            {
                case "SRAM":
                    return "bram.srm";
                case "EEPROM":
                    return "bram.eep";
                case "FLASH":
                    return "bram.fla";
                default:
                    return "bram.sav";
            }
        }
    }
}
