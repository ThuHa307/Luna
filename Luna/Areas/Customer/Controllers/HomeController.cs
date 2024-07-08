using Luna.Data;
using Luna.Models;
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
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            ViewData["userId"] = userId;
            ///
            var userApplication = _dbContext.ApplicationUser
                                .Where(u => u.Id == userId)
                                .FirstOrDefault();
            if (userApplication != null) 
            {
                HttpContext.Session.SetString("wallet", userApplication.Wallet.ToString());
            }
            
            /////
            var messages = _dbContext.ChatMessages
                           .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                           .OrderBy(m => m.Timestamp)
                           .ToList();
            ViewData["consultantId"] = _globalService.GetConsultantId();
            var feedbacks = _dbContext.Feedbacks.Include(f => f.User).Where(f => f.Show == true).ToList();
            ViewBag.Feedbacks = feedbacks;
            ViewBag.totalRoom = _dbContext.Rooms.Count();
            // Lấy danh sách người dùng từ database
            List<ApplicationUser> listaccount = await _dbContext.ApplicationUser.ToListAsync();
            List<ApplicationUser> receptionists = new List<ApplicationUser>();

            foreach (var user in listaccount)
            {
                // Lấy role
                var roles = await _userManager.GetRolesAsync(user);
                // Nếu là Receptionist và Email đã xác nhận thì add vào list
                if (roles.Contains("Receptionist") && user.EmailConfirmed)
                {
                    receptionists.Add(user);
                }
            }

            // Đếm số lượng receptionists
            int receptionistCount = receptionists.Count;
            ViewBag.totalStaff = receptionistCount;
            ViewBag.totalCustomer = _dbContext.Customers.Count();

            return View(messages);
        }
    }
}
