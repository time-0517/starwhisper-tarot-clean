using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 占卜.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;


namespace 占卜.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        // ⭐ 用 DI 注入 DbContext（Azure / 本機都通）
        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // 1. 顯示登入頁面
        [AllowAnonymous]
        public IActionResult Login()
        {
            // ⭐ 從資料庫撈 TarotCards，丟給 View
            ViewBag.TarotCards = _db.TarotCards
                .Select(t => new TarotCard
                {
                    Id = t.Id,
                    Name = t.Name,
                    ImageName = t.ImageName
                })
                .ToList();

            return View();
        }

        // 2. 處理登入
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginModel model)
        {
            string passwordHash = HashPassword(model.Password ?? "");

            bool ok = _db.Users.Any(u =>
                u.Username == model.Username &&
                u.PasswordHash == passwordHash);

            if (ok)
            {
                var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, model.Username!)
    };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal
                );

                var user = _db.Users.First(u =>
    u.Username == model.Username &&
    u.PasswordHash == passwordHash);

                // 存進 Session
                HttpContext.Session.SetInt32("UserId", user.Id);

                return RedirectToAction("Index", "Home");

            }


            ViewBag.Error = "帳號或密碼錯誤";
            ViewBag.SkipIntro = true;

            // 重新丟牌資料，避免 ViewBag 為 null
            ViewBag.TarotCards = _db.TarotCards.ToList();

            return View(model);
        }

        // 3. 顯示註冊
        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewBag.TarotCards = _db.TarotCards.ToList();
            return View();
        }

        // 4. 處理註冊
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register(UserLoginModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) ||
                string.IsNullOrWhiteSpace(model.Password))
            {
                ViewBag.Error = "帳號與密碼不能為空";
                return View(model);
            }

            if (_db.Users.Any(u => u.Username == model.Username))
            {
                ViewBag.Error = "這個帳號已經存在";
                return View(model);
            }

            _db.Users.Add(new User
            {
                Username = model.Username,
                PasswordHash = HashPassword(model.Password)
            });

            _db.SaveChanges();

            return RedirectToAction("Login");
        }

        // 密碼雜湊
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
