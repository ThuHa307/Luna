using Luna.Data;
using Luna.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Luna.Areas.Customer.Controllers
{
    [Area("Customer")]
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _dbContext;
        private readonly GlobalService _globalService;
        public HomeController(UserManager<IdentityUser> userManager, AppDbContext dbContext, GlobalService globalService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _globalService = globalService;
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            ViewData["userId"] = userId;
            ///
            var userApplication = _dbContext.ApplicationUser
                                .Where(u => u.Id == userId)
                                .FirstOrDefault();
            
            HttpContext.Session.SetString("wallet", userApplication.Wallet.ToString());
            /////
            var messages = _dbContext.ChatMessages
                           .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                           .OrderBy(m => m.Timestamp)
                           .ToList();
            ViewData["consultantId"] = _globalService.GetConsultantId();
            return View(messages);
        }
    }
}
