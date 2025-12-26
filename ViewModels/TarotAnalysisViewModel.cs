namespace 占卜.ViewModels
{
    public class TarotAnalysisCardViewModel
    {
        public int TarotCardId { get; set; }
        public string Name { get; set; }
        public string ImageName { get; set; }
        public string Orientation { get; set; }   // upright / reversed
        public string Position { get; set; }      // 過去 / 現在 / 未來 / 自己 / 對方 / 關係走向 / 指引
        public string KeywordsUp { get; set; }
        public string KeywordsRev { get; set; }
        public string AiDetail { get; set; }      // 這一張牌的 AI 解讀
    }

    public class TarotAnalysisViewModel
    {
        public int DrawSessionId { get; set; }    // 對應 DrawSessions.Id
        public string Question { get; set; }
        public string Mode { get; set; }          // single / timeline / relationship
        public string AiSummary { get; set; }     // 整體總結
        public List<TarotAnalysisCardViewModel> Cards { get; set; } = new();
    }
}
