using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using 占卜.Models;
using 占卜.ViewModels;

namespace 占卜.Controllers
{
    public class TarotController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public TarotController(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;

            _http = new HttpClient
            {
                BaseAddress = new Uri("https://api.openai.com/v1/")
            };

            var apiKey = _config["OpenAI:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);
            }
        }
        [HttpGet]
        public IActionResult Introduction()
        {
            return View();
        }

        // 78 張牌列表
        // =============================
        // 78 張牌列表
        // GET: /Tarot/CardList
        // =============================
        [HttpGet]
        public IActionResult CardList()
        {
            // ✅ 1. 先從資料庫撈出來（只做 SQL 能處理的事）
            var cards = _db.TarotCards
                .OrderBy(c => c.Id)
                .ToList();   // ⬅️ 關鍵在這一行

            // ✅ 2. 再用 C# 處理字串（Trim / Split 都安全）
            var cardList = cards.Select(c => new TarotCard
            {
                Id = c.Id,
                Name = c.Name,
                ImageName = c.ImageName,

                KeywordsUp = string.IsNullOrWhiteSpace(c.KeywordsUp)
                    ? null
                    : string.Join(", ",
                        c.KeywordsUp
                            .Split(',')
                            .Select(s => s.Trim())
                            .Take(2)),

                KeywordsRev = string.IsNullOrWhiteSpace(c.KeywordsRev)
                    ? null
                    : string.Join(", ",
                        c.KeywordsRev
                            .Split(',')
                            .Select(s => s.Trim())
                            .Take(2))
            }).ToList();

            return View(cardList);
        }



        // =============================
        // 抽牌頁
        // =============================
        [HttpGet]
        public IActionResult DrawCards()
        {
            var cards = _db.TarotCards
                .OrderBy(c => c.Id)
                .ToList();

            return View(cards);
        }

        // =============================
        // GET：避免直接打 /Tarot/Analysis 404
        // =============================
        [HttpGet]
        public IActionResult Analysis()
        {
            return RedirectToAction("DrawCards");
        }





        // =============================
        // POST：真正的占卜解析
        // =============================
        [HttpPost]
        public async Task<IActionResult> Analysis(
            string question,
            string mode,
            string cardsJson)
        {
            if (string.IsNullOrWhiteSpace(cardsJson))
                return BadRequest("沒有收到抽牌資料");

            var pickedCards = JsonSerializer.Deserialize<List<PickedCardDto>>(
                cardsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (pickedCards == null || !pickedCards.Any())
                return BadRequest("抽牌資料為空");

            // === 1. 取牌資料 ===
            var cardIds = pickedCards.Select(c => c.Id).ToList();
            var tarotCards = await _db.TarotCards
                .Where(c => cardIds.Contains(c.Id))
                .ToListAsync();

            // === 2. 呼叫 OpenAI（只一次） ===
            // === 1. 呼叫 OpenAI（先）===
            var aiResult = await CallOpenAiAsync(
                question,
                mode,
                pickedCards,
                tarotCards
            );

            if (aiResult == null)
            {
                return Content("AI 暫時無法回應，請稍後再試");
            }

            // === 2. 建立 DrawSession ===
            // 取得目前使用者 ID（未登入則為 null）
            int? userId = HttpContext.Session.GetInt32("UserId");


            var session = new DrawSession
            {
                Question = question,
                DrawMode = mode,
                DrawCount = pickedCards.Count,
                CreatedAt = DateTime.UtcNow,
                AiSummary = aiResult.Summary,
                UserId = userId ?? 0  // ⭐ 登入者會寫入 userId，未登入則寫 0
            };


            _db.DrawSessions.Add(session);
            await _db.SaveChangesAsync(); // ⭐ 這行一定要先存！

            // === 3. 存 DrawCards（含 AiDetail）===
            foreach (var (pc, index) in pickedCards.Select((c, i) => (c, i)))
            {
                var aiCard = aiResult.Cards
                    .FirstOrDefault(x => x.CardId == pc.Id);

                _db.DrawCards.Add(new DrawCard
                {
                    DrawSessionId = session.Id,
                    TarotCardId = pc.Id,
                    PositionIndex = index,
                    Orientation = pc.Orientation,
                    PositionName = pc.Position,
                    AiDetail = aiCard?.Detail
                });
            }

            await _db.SaveChangesAsync();


            // === 5. 組 ViewModel ===
            var vm = new TarotAnalysisViewModel
            {
                Question = question,
                Mode = mode,
                AiSummary = aiResult.Summary,
                Cards = new List<TarotAnalysisCardViewModel>()
            };

            foreach (var pc in pickedCards)
            {
                var card = tarotCards.First(c => c.Id == pc.Id);
                var detail = aiResult.Cards
                    .FirstOrDefault(x => x.CardId == pc.Id)?.Detail;

                vm.Cards.Add(new TarotAnalysisCardViewModel
                {
                    TarotCardId = card.Id,
                    Name = card.Name,
                    ImageName = card.ImageName,
                    KeywordsUp = card.KeywordsUp,
                    KeywordsRev = card.KeywordsRev,
                    Orientation = pc.Orientation,
                    Position = pc.Position,
                    AiDetail = detail
                });
            }

            return View(vm);
        }


        // =============================
        // OpenAI
        // =============================
        private async Task<TarotAiJson?> CallOpenAiAsync(
            string question,
            string mode,
            List<PickedCardDto> pickedCards,
            List<TarotCard> tarotCards)
        {
            var sb = new StringBuilder();
            sb.AppendLine("你是一位專業塔羅牌占卜師，請用繁體中文回答。");
            sb.AppendLine($"問題：{question}");
            sb.AppendLine($"牌陣：{mode}");
            sb.AppendLine("抽到的牌：");

            foreach (var pc in pickedCards)
            {
                var card = tarotCards.First(c => c.Id == pc.Id);
                sb.AppendLine($"- {card.Name}（{pc.Orientation}，{pc.Position}）");
            }

            sb.AppendLine(@"
請「只」回傳一段【合法 JSON 字串】，不得包含任何多餘文字。
嚴格遵守以下規則：

1. 不可使用 ``` 或 ```json
2. 不可在 JSON 前後加說明文字
3. 回傳內容必須能直接被 JSON 解析
4. 只能包含 summary 與 cards 欄位

JSON 格式如下：
{
  ""summary"": ""整體占卜總結（請用自然段落文字，不要包含 JSON）"",
  ""cards"": [
    {
      ""cardId"": 1,
      ""detail"": ""此張牌在本次占卜中的具體解讀""
    }
  ]
}
");


            var req = new
            {
                model = _config["OpenAI:Model"] ?? "gpt-4o-mini",
                messages = new[]
                {
            new { role = "user", content = sb.ToString() }
        }
            };

            using var httpReq = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
            httpReq.Content = new StringContent(
                JsonSerializer.Serialize(req),
                Encoding.UTF8,
                "application/json");

            var res = await _http.SendAsync(httpReq);

            // 非成功狀態（429 / 401 / 500）
            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await res.Content.ReadAsStringAsync();

            var parsed = JsonSerializer.Deserialize<ChatCompletionResponse>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var content = parsed?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            // ✅ 移除 ```json 與 ```
            content = content.Trim();
            if (content.StartsWith("```"))
            {
                content = content
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();
            }

            try
            {
                return JsonSerializer.Deserialize<TarotAiJson>(
                    content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch
            {
                // JSON 解析失敗 → 當成純文字 summary
                return new TarotAiJson
                {
                    Summary = content
                };
            }
        }

        private class ChatCompletionResponse
        {
            public List<Choice> Choices { get; set; } = new();

            public class Choice
            {
                public Message Message { get; set; } = new();
            }

            public class Message
            {
                public string Content { get; set; } = "";
            }
        }





[Authorize]
public IActionResult History()
{
    int? userId = HttpContext.Session.GetInt32("UserId");

    var list = _db.DrawSessions
        .Where(x => x.UserId == userId)
        .OrderByDescending(x => x.CreatedAt)
        .Select(s => new TarotHistoryItemViewModel
        {
            SessionId = s.Id,
            Question = s.Question,
            Mode = s.DrawMode,
            DrawCount = s.DrawCount,
            CreatedAt = s.CreatedAt,
            AiSummary = s.AiSummary
        })
        .ToList();

    return View(list);
}




        [Authorize]
        [HttpGet]
        public async Task<IActionResult> HistoryDetail(int id)
        {
            // 1. 找這次的抽牌紀錄 + 卡片
            var session = await _db.DrawSessions
                .Include(s => s.DrawCards)
                    .ThenInclude(dc => dc.TarotCard)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
                return NotFound();

            // 2. 組成跟「占卜解析」一樣用的 ViewModel
            var vm = new TarotAnalysisViewModel
            {
                Question = session.Question,
                Mode = session.DrawMode,
                AiSummary = session.AiSummary,
                Cards = session.DrawCards
                    .OrderBy(c => c.PositionIndex)
                    .Select(c => new TarotAnalysisCardViewModel
                    {
                        TarotCardId = c.TarotCardId,
                        Name = c.TarotCard.Name,
                        ImageName = c.TarotCard.ImageName,
                        KeywordsUp = c.TarotCard.KeywordsUp,
                        KeywordsRev = c.TarotCard.KeywordsRev,
                        Orientation = c.Orientation,
                        Position = c.PositionName,
                        AiDetail = c.AiDetail
                    })
                    .ToList()
            };

            // 3. 直接重用 Analysis.cshtml 這個頁面來顯示
            return View("Analysis", vm);
        }


        [HttpPost]
        public async Task<IActionResult> ClearHistory(List<int> selectedIds)
        {
            // 🔒 沒選任何東西
            if (selectedIds == null || !selectedIds.Any())
            {
                return RedirectToAction("History");
            }

            // 找出要刪的 session
            var sessions = await _db.DrawSessions
                .Where(s => selectedIds.Contains(s.Id))
                .ToListAsync();

            // 找出對應的牌
            var cards = await _db.DrawCards
                .Where(c => selectedIds.Contains(c.DrawSessionId))
                .ToListAsync();

            _db.DrawCards.RemoveRange(cards);
            _db.DrawSessions.RemoveRange(sessions);

            await _db.SaveChangesAsync();

            return RedirectToAction("History");
        }


    }
}
