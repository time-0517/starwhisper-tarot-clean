// Models/PickedCardDto.cs
using System.Text.Json.Serialization;

namespace 占卜.Models
{
    public class PickedCardDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        // "upright" / "reversed"
        [JsonPropertyName("orientation")]
        public string Orientation { get; set; } = "";

        // "過去"、"現在"、"未來"、"自己"、"對方"、"關係走向"、"指引"
        [JsonPropertyName("position")]
        public string Position { get; set; } = "";
    }
}
