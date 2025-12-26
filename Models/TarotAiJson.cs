// Models/TarotAiJson.cs
using System.Collections.Generic;

namespace 占卜.Models
{
    // 用來接 AI 回傳的 JSON
    public class TarotAiJson
    {
        public string Summary { get; set; } = "";
        public List<TarotAiJsonCard> Cards { get; set; } = new();
    }

    public class TarotAiJsonCard
    {
        public int CardId { get; set; }
        public string Detail { get; set; } = "";
    }
}
