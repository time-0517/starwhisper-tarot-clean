using System;
using System.Collections.Generic;

namespace 占卜.Models
{
    public class DrawSession
    {
        public int Id { get; set; }

        public int? UserId { get; set; }   // 之後登入用
        public string Question { get; set; } = "";
        public string DrawMode { get; set; } = "";
        public int DrawCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? AiSummary { get; set; }

        public ICollection<DrawCard> DrawCards { get; set; } = new List<DrawCard>();
    }
}
