namespace RomDownloader
{
    internal static class DirectoryMapper
    {
        public static string? GetDirectory(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
            {
                return null;
            }

            Dictionary<string, string> mapping = new(StringComparer.OrdinalIgnoreCase)
        {
            // Atari Systems
            { "Atari 2600", "A2600" },
            { "Atari 5200", "A5200" },
            { "Atari 7800", "A7800" },
            { "Atari 800", "A800" },
            { "Atari ST", "ATARIST" },
            { "Atari Lynx", "LYNX" },

            // Nintendo Systems
            { "Nintendo", "FC" },
            { "Nintendo Entertainment System", "FC" },
            { "NES", "FC" },
            { "Super Nintendo", "SFC" },
            { "Super Nintendo Entertainment System", "SFC" },
            { "SNES", "SFC" },
            { "Nintendo 64", "N64" },
            { "Nintendo DS", "NDS" },
            { "Game Boy", "GB" },
            { "Game Boy Color", "GBC" },
            { "Game Boy Advance", "GBA" },
            { "Virtual Boy", "VB" },

            // Sega Systems
            { "Master System", "SMS" },
            { "Sega Master System", "SMS" },
            { "Genesis", "MD" },
            { "Sega Genesis", "MD" },
            { "Sega CD", "MDCD" },
            { "Sega 32X", "SEGA32X" },
            { "Saturn", "SATURN" },
            { "Sega Saturn", "SATURN" },
            { "Game Gear", "GG" },

            // Other Systems
            { "TurboGrafx-16", "PCE" },
            { "TurboGrafx-CD", "PCECD" },
            { "PlayStation", "PS" },
            { "PlayStation Portable", "PSP" },
            { "Dreamcast", "DREAMCAST" },
            { "Lynx", "LYNX" },
        };

            // Trim and normalize the input system name
            systemName = systemName.Trim();

            // Attempt to get the directory from the mapping
            if (mapping.TryGetValue(systemName, out string directory))
            {
                return directory;
            }
            else
            {
                // Return null if no applicable directory is found
                return null;
            }
        }
    }
}