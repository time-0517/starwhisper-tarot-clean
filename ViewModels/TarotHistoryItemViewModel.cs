namespace 占卜.ViewModels
{
    public class TarotHistoryItemViewModel
    {
        public int SessionId { get; set; }
        public string Question { get; set; } = "";
        public string Mode { get; set; } = "";
        public int DrawCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AiSummary { get; set; }

        public bool Selected { get; set; }
    }
}
