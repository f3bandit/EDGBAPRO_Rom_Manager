using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using EDGBAPRO_Rom_Manager.Models;

namespace EDGBAPRO_Rom_Manager.Services
{
    public class SaveTypeDatabase
    {
        private readonly List<SaveDbEntry> _entries;
        private readonly Dictionary<string, string> _exactMap;

        public SaveTypeDatabase(string dbPath)
        {
            if (!File.Exists(dbPath))
            {
                _entries = new List<SaveDbEntry>();
                _exactMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                return;
            }

            var json = File.ReadAllText(dbPath);
            _entries = JsonSerializer.Deserialize<List<SaveDbEntry>>(json) ?? new List<SaveDbEntry>();
            _exactMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in _entries)
            {
                AddKey(entry.Title, entry.SaveType);

                foreach (var alias in entry.Aliases ?? Enumerable.Empty<string>())
                {
                    AddKey(alias, entry.SaveType);
                }
            }
        }

        public string LookupByRomName(string romName)
        {
            var candidates = BuildCandidates(romName);

            // exact normalized lookup first
            foreach (var candidate in candidates)
            {
                if (_exactMap.TryGetValue(candidate, out var saveType))
                {
                    return saveType;
                }
            }

            // contains / startswith fallback for No-Intro style names
            foreach (var candidate in candidates)
            {
                foreach (var entry in _entries)
                {
                    var title = Normalize(entry.Title);
                    if (title == candidate ||
                        candidate.StartsWith(title + " ", StringComparison.OrdinalIgnoreCase) ||
                        title.StartsWith(candidate + " ", StringComparison.OrdinalIgnoreCase) ||
                        candidate.Contains(title, StringComparison.OrdinalIgnoreCase))
                    {
                        return entry.SaveType;
                    }

                    foreach (var alias in entry.Aliases ?? Enumerable.Empty<string>())
                    {
                        var a = Normalize(alias);
                        if (a == candidate ||
                            candidate.StartsWith(a + " ", StringComparison.OrdinalIgnoreCase) ||
                            a.StartsWith(candidate + " ", StringComparison.OrdinalIgnoreCase) ||
                            candidate.Contains(a, StringComparison.OrdinalIgnoreCase))
                        {
                            return entry.SaveType;
                        }
                    }
                }
            }

            return "UNKNOWN";
        }

        private void AddKey(string raw, string saveType)
        {
            var normalized = Normalize(raw);
            if (!string.IsNullOrWhiteSpace(normalized) && !_exactMap.ContainsKey(normalized))
            {
                _exactMap[normalized] = saveType;
            }
        }

        private static IEnumerable<string> BuildCandidates(string romName)
        {
            var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string original = romName ?? string.Empty;
            results.Add(Normalize(original));

            string stripped = StripMetadata(original);
            results.Add(Normalize(stripped));

            string withoutRev = Regex.Replace(stripped, @"\brev\s*\d+\b", "", RegexOptions.IgnoreCase).Trim();
            results.Add(Normalize(withoutRev));

            string beforeDash = Regex.Replace(stripped, @"\s*-\s*\b(the|usa|europe|japan)\b.*$", "", RegexOptions.IgnoreCase).Trim();
            results.Add(Normalize(beforeDash));

            return results.Where(s => !string.IsNullOrWhiteSpace(s));
        }

        private static string StripMetadata(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string s = value;
            s = Regex.Replace(s, @"\([^)]*\)", " ");
            s = Regex.Replace(s, @"\[[^\]]*\]", " ");
            s = Regex.Replace(s, @"\{[^}]*\}", " ");
            s = Regex.Replace(s, @"\b(v|rev)\s*\d+(\.\d+)?\b", " ", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\b(proto|beta|sample|demo|aftermarket|unlicensed|pirate)\b", " ", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\s+", " ").Trim();
            return s;
        }

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string s = StripMetadata(value).ToLowerInvariant();
            s = s.Replace("&", " and ");
            s = s.Replace("+", " plus ");
            s = s.Replace("/", " ");
            s = s.Replace("-", " ");
            s = s.Replace("pokémon", "pokemon");
            s = s.Replace("’", "'");
            var chars = s.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray();
            s = new string(chars);
            while (s.Contains("  "))
                s = s.Replace("  ", " ");
            return s.Trim();
        }

        public static string GetBramFileName(string saveType)
        {
            return saveType.ToUpperInvariant() switch
            {
                "SRAM" => "bram.srm",
                "EEPROM" => "bram.eep",
                "FLASH" => "bram.fla",
                _ => "bram.sav"
            };
        }
    }
}
