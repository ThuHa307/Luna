using Luna.Areas.Chat.Models;
using Luna.Areas.Customer.Controllers.VNPaylib;
using Luna.Areas.Customer.Controllers.VNPaylib.Services;
using Luna.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Luna.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class VNPayController : Controller
    {
        public readonly IVnPayService _vnPayService;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly AppDbContext _context;
		public VNPayController(AppDbContext context, IVnPayService vnPayService, UserManager<IdentityUser> userManager)
        {
            _vnPayService = vnPayService;
			_userManager = userManager;
            _context = context;
		}
        public IActionResult Index(decimal wallet, decimal deposits)
        {
            ViewBag.Wallet = wallet;
            ViewBag.Deposits = deposits;
            return View();
        }
        public IActionResult Payment(decimal amount) 
        {
            var vnPayModel = new VnPaymentRequestModel
            {
                Amount = (double)amount,
                CreatedDate = DateTime.Now,
                Description = $"Nạp tiền",
                FullName = "null",
                Id = new Random().Next(1000, 100000)
            };
            HttpContext.Session.SetInt32("Amount",(int)vnPayModel.Amount);
            return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
        }
        [Authorize]
        public async Task<IActionResult> PaymentCallBackAsync()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"VNPay payment error: {response.VnPayResponseCode}";
				return RedirectToAction("Create", "HotelOrders", new { area = "Customer" });
			}


			// + tiền nạp vào ví
			decimal wallet;
			var user = await _userManager.GetUserAsync(User);
			var userId = user?.Id;
			var userApplication = _context.ApplicationUser
								.Where(u => u.Id == userId)
								.FirstOrDefault();
			if (userApplication == null)
			{
				return NotFound("Lỗi ttrong database á!");
			}
			wallet = (decimal)HttpContext.Session.GetInt32("Amount");

			userApplication.Wallet += wallet;
			//Cập nhật tiền trong ví
			await _context.SaveChangesAsync();
			TempData["Message"] = $"VNPay deposit successful";
            return RedirectToAction("Create", "HotelOrders", new { area = "Customer" });
        }
    }
}
