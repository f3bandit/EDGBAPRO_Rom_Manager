using System.Collections.Generic;

namespace EDGBAPRO_Rom_Manager.Models
{
    public class SaveDbEntry
    {
        public string Title { get; set; } = "";
        public List<string> Aliases { get; set; } = new();
        public string SaveType { get; set; } = "UNKNOWN";
    }
}
