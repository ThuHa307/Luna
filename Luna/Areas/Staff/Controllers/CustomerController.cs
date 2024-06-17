using Luna.Areas.Staff.Models;
using Luna.Data;
using Luna.Models;
using Luna.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using CustomerModel = Luna.Models.Customer;
namespace Luna.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Admin, Receptionist")]
    public class CustomerController : Controller
    {
        private readonly AppDbContext _dbContext;
        

        public CustomerController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Create(int orderId, int roomId)
        {
            ViewData["orderId"] = orderId;
            ViewData["roomId"] = roomId;
            return View();
        }
        [HttpPost]
        public IActionResult Create(CustomerModel customerModel)
        {
            var customer = new CustomerModel
            {
                OrderId = customerModel.OrderId,
                RoomId = customerModel.RoomId,
                CusName = customerModel.CusName,
                Cccd = customerModel.Cccd,
                DateOfBirth = customerModel.DateOfBirth,
                Address = customerModel.Address,
                Genre = customerModel.Genre
            };
            _dbContext.Customers.Add(customer);
            _dbContext.SaveChanges();

            return RedirectToAction("ConfirmCI", "DemoRO", new { orderId = customer.OrderId, roomId = customer.RoomId });
        }
        public IActionResult Edit(int? customerId)
        {
            if (customerId == null || customerId == 0)
            {
                return NotFound();
            }

            var customer = _dbContext.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        public IActionResult Edit(CustomerModel obj)
        {
            _dbContext.Customers.Update(obj); 
            _dbContext.SaveChanges();
            ViewData["StatusMessage"] = "Cập nhật thông tin thành công!";
            return View(obj);
        }

        [HttpPost]
        public IActionResult Delete(int? customerId, int orderId, int roomId)
        {
            var customer = _dbContext.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer != null)
            {
                _dbContext.Customers.Remove(customer);
                _dbContext.SaveChanges();
                return RedirectToAction("ConfirmCI", "DemoRO", new { orderId = orderId, roomId = roomId });
            }

            // Handle the case where the customer is not found
            return NotFound();
                       
        }
    }
}
