using Luna.Data;
using Luna.Models;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;

namespace Luna.Areas.Admin.Controllers
{
    [Area("admin")]

    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        // Use this constructor for dependency injection
        [ActivatorUtilitiesConstructor]
        public HomeController(AppDbContext db)
        {
            _db = db;
        }


        public IActionResult Index()
        {
            return View();
        }

    
       

        
    


       

    }
}
