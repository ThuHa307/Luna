using Luna.Areas.Staff.Models;
using Luna.Data;
using Luna.Models;
using Luna.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Luna.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Admin, Receptionist")]
    public class DemoROController : Controller
    {
        private readonly AppDbContext _dbContext;

        public DemoROController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index(int id)
        {
            var roomOrders = _dbContext.RoomOrders.Where(r => r.OrderId == id).ToList();
            ViewBag.OrderId = id;
            return View(roomOrders);
        }

        public IActionResult ConfirmCI(string orderId, string roomId)
        {
            var customers = _dbContext.Customers.Where(c => c.OrderId == Int32.Parse(orderId) && c.RoomId == Int32.Parse(roomId)).ToList();
            RoomOrderVM roomOrderVM = new()
            {
                OrderId = Int32.Parse(orderId), RoomId = Int32.Parse(roomId), Customers = customers
            };
            var roomOrder = _dbContext.RoomOrders.FirstOrDefault(ro => ro.RoomId == roomOrderVM.RoomId && ro.OrderId == roomOrderVM.OrderId);
            if (roomOrder != null)
            {
                ViewData["CheckedIn"] = roomOrder.ConfirmCheckIn;
            }
            return View(roomOrderVM);
        }
        [HttpPost]
        public IActionResult ConfirmCI(RoomOrderVM roomOrderVM)
        {
            var roomOrder = _dbContext.RoomOrders.FirstOrDefault(ro => ro.RoomId  == roomOrderVM.RoomId && ro.OrderId == roomOrderVM.OrderId);
            if (roomOrder == null)
            {
                return NotFound();
            }
            roomOrder.ConfirmCheckIn = DateTime.Now;
            _dbContext.RoomOrders.Update(roomOrder);
            _dbContext.SaveChanges();
            return RedirectToAction("Index", new {id = roomOrderVM.OrderId });
        }

        public IActionResult ConfirmCO(int? orderId, int? roomId)
        {
            if(roomId == null || orderId == null)
            {
                return NotFound();
            }
            var roomOrder = _dbContext.RoomOrders.FirstOrDefault(ro => ro.RoomId == roomId && ro.OrderId == orderId);
            if (roomOrder == null)
            {
                return NotFound();
            }
            roomOrder.ConfirmCheckOut = DateTime.Now;
            _dbContext.RoomOrders.Update(roomOrder);
            _dbContext.SaveChanges();
            return RedirectToAction("Index", new { id = orderId } );
        }

        
    }
}
