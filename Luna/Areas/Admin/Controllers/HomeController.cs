using Luna.Data;
using Luna.Models;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;

namespace Luna.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin")]
    [Route("admin/homeadmin")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        // Use this constructor for dependency injection
        [ActivatorUtilitiesConstructor]
        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        [Route("")]
        [Route("index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("staff")]
        public IActionResult StaffAccount()
        {
            List<ApplicationUser> liststaff = _db.ApplicationUser.ToList();
            return View(liststaff);
        }

        [Route("add")]
        public IActionResult Addnew()
        {
            return View();
        }


        [Route("update")]
        public IActionResult UpdateAccount()
        {

            return View();
        }

    }
}
