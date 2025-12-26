using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace 占卜.Controllers
{
    [Authorize] // 👈 要登入才能進
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

       
    }
}
