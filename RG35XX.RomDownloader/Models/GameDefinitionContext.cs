using System.Text.Json.Serialization;

namespace RomDownloader.Models
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(GameDefinition))]
    [JsonSerializable(typeof(GameDefinition[]))]
    [JsonSerializable(typeof(List<GameDefinition>))]
    [JsonSerializable(typeof(Dictionary<string, List<GameDefinition>>))]
    [JsonSerializable(typeof(ImageDefinition))]
    [JsonSerializable(typeof(ImageDefinition[]))]
    [JsonSerializable(typeof(List<ImageDefinition>))]
    public partial class GameDefinitionContext : JsonSerializerContext
    {
    }
}