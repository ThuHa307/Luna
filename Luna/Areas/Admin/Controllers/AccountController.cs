using Luna.Data;
using Luna.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Connections;

namespace Luna.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {

        private readonly AppDbContext _db;

        // Use this constructor for dependency injection
        [ActivatorUtilitiesConstructor]
        public AccountController(AppDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<ApplicationUser> liststaff = _db.ApplicationUser.ToList();
            return View(liststaff);
        }
        public IActionResult AddAccount()
        {
            return View();
        }
    }
}
