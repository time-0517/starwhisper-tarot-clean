namespace 占卜.Models
{
    public class TarotCard
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // 牌名
        public string ImageName { get; set; } = string.Empty; // 圖片檔名
        // 其他欄位(描述等)暫時不用，因為介紹頁列表只需要顯示圖片和名字
        public string KeywordsUp { get; set; }
        public string KeywordsRev { get; set; }

        public string Description { get; set; }

    }
}