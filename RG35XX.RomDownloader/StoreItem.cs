using System.Text.Json.Serialization;

namespace RG35XX.RomDownloader
{
    internal class StoreItem
    {
        [JsonPropertyName("entry_point")]
        public string EntryPoint { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}