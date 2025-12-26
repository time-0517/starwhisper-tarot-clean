namespace 占卜.Models
{
    public class DrawCard
    {
        public int Id { get; set; }

        public int DrawSessionId { get; set; }
        public DrawSession DrawSession { get; set; } = null!;

        public int TarotCardId { get; set; }
        public TarotCard TarotCard { get; set; } = null!;

        public int PositionIndex { get; set; }      // 第幾張
        public string PositionName { get; set; } = ""; // 過去 / 現在 / 未來
        public string Orientation { get; set; } = "";  // upright / reversed

        public string? AiDetail { get; set; }
    }
}
