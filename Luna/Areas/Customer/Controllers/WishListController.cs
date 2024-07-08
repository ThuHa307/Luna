using Luna.Data;
using Luna.Models;
using Luna.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Luna.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = Roles.Role_Customer)]
    public class WishListController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _dbContext;

        public WishListController(UserManager<IdentityUser> userManager, AppDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Add(int typeId)
        {
            var referrerUrl = Request.Headers["Referer"].ToString();
            var userId = _userManager.GetUserId(User);
            if(userId != null)
            {
                var duplicate = _dbContext.WishLists.Where(wl => wl.UserId == userId && wl.TypeId == typeId).FirstOrDefault();
                if(duplicate == null)
                {
                    WishList wl = new() { UserId = userId, TypeId = typeId };
                    _dbContext.WishLists.Add(wl);
                }
                else
                {
                    _dbContext.WishLists.Remove(duplicate);
                }
                _dbContext.SaveChanges();
            }
            if (!string.IsNullOrEmpty(referrerUrl))
            {
                return Redirect(referrerUrl);
            }

            return RedirectToAction("Room", "Room", new { area = "Admin", page = 1 });
        }
    }
}
