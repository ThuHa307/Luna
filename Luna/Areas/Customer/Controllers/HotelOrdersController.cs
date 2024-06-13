using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Luna.Data;
using Luna.Models;
using Microsoft.AspNetCore.Identity;
using Luna.Areas.Customer.Models;
using Org.BouncyCastle.X509.Store;

namespace Luna.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HotelOrdersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public HotelOrdersController(AppDbContext context
            , UserManager<IdentityUser> userManager
            )
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Customer/Order
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter, int? pageNumber)
        {
            //Lấy id đang đăng nhập
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;
            var hotelOrders = from s in _context.HotelOrders
                              select s;
            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            else
            {
                var role = await _userManager.GetRolesAsync(user);
                ViewBag.UserLoggedRoles = role.ToList();
                ViewBag.UserLoggedId = userId;
                if (role.Contains("Customer"))
                {
                    hotelOrders = from s in _context.HotelOrders
                                  where s.Id == userId
                                  select s;
                }             
            }
            ////////////////////
            ViewData["CurrentSort"] = sortOrder;
            ViewData["IDSortParm"] = String.IsNullOrEmpty(sortOrder) ? "ID" : "";
            ViewData["StatusSortParm"] = sortOrder == "OrderStatus" ? "status_desc" : "Status";
            ViewData["DepositsSortParm"] = sortOrder == "Deposits" ? "Deposits_desc" : "Deposits";
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;
            
            if (!String.IsNullOrEmpty(searchString))
            {
                //
                DateOnly searchDate;
                bool isDate = DateOnly.TryParse(searchString, out searchDate);
                //
                hotelOrders = hotelOrders.Where(s =>
                (isDate && s.OrderDate == searchDate) || 
                Convert.ToString(s.OrderId).Contains(searchString) ||
                Convert.ToString(s.Id).Contains(searchString) ||
                (s.Deposits != null && s.Deposits.ToString().Contains(searchString)) ||
                s.OrderStatus.Contains(searchString))
                .Include(s => s.User)
                ;
            }
            switch (sortOrder)
            {
                case "ID":
                    hotelOrders = hotelOrders.OrderBy(s => s.OrderId);
                    break;
                case "Status":
                    hotelOrders = hotelOrders.OrderBy(s => s.OrderStatus);
                    break;
                case "status_desc":
                    hotelOrders = hotelOrders.OrderByDescending(s => s.OrderStatus);
                    break;
                case "Deposits":
                    hotelOrders = hotelOrders.OrderBy(s => s.Deposits);
                    break;
                case "Deposits_desc":
                    hotelOrders = hotelOrders.OrderByDescending(s => s.Deposits);
                    break;
                default:
                    hotelOrders = hotelOrders.OrderByDescending(s => s.OrderId);
                    break;
            }
            int pageSize = 10;

            return View(await PaginatedList<HotelOrder>.CreateAsync(hotelOrders.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Customer/Order/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var hotelOrder = await _context.HotelOrders
                            .Include(h => h.User)
                            .Include(h => h.RoomOrders)
                            .Include(h => h.OrderDetails)
                                .ThenInclude(od => od.Type)
                            .FirstOrDefaultAsync(m => m.OrderId == id);
            if (hotelOrder == null)
            {
                return NotFound();
            }
            int stayDaysCount = _context.RoomOrders
                                .Where(ro => ro.OrderId == id && ro.CheckIn.HasValue && ro.CheckOut.HasValue)
                                .Select(ro => (ro.CheckOut.Value.DayNumber - ro.CheckIn.Value.DayNumber) + 1)
                                .FirstOrDefault();
            ViewBag.stayDaysCount = stayDaysCount;
            //Order room
            List<(string? TypeName, int? NumberOfRoom, double TypePrice, double Discount)> roomTypeInfos = new List<(string?, int?, double, double)>();

            var orderDetails = _context.OrderDetails
                                        .Where(od => od.OrderId == id)
                                        .ToList();
            
            foreach (var item in orderDetails)
            {
                string? typeName = _context.RoomTypes
                                .Where(rt => rt.TypeId == item.TypeId)
                                .Select(rt => rt.TypeName)
                                .FirstOrDefault();
                int? numberOfRoom = item.NumberOfRoom;
                double? typePriceNullable = _context.RoomTypes
                                            .Where(rt => rt.TypeId == item.TypeId)
                                            .Select(rt => (double?)rt.TypePrice)
                                            .FirstOrDefault();

                double typePrice = typePriceNullable ?? 0;
                //Lấy id giảm giá nếu có
                var promotionId = await _context.RoomPromotions
                                .Where(rp => rp.TypeId == item.TypeId
                                        && _context.RoomOrders.Any(ro => ro.OrderId == hotelOrder.OrderId
                                        && rp.StartDate <= ro.CheckIn
                                        && rp.EndDate >= ro.CheckOut))
                                .Select(rp => rp.PromotionId)
                                .FirstOrDefaultAsync();
                //Tìm discount nếu có
                double discount = 0;
                if (promotionId != 0)
                {
                    discount = await _context.Promotions
                        .Where(p => p.PromotionId == promotionId && p.IsActive == true)
                        .Select(p => p.Discount)
                        .FirstOrDefaultAsync() ?? 0;
                }
                // Tạo và thêm một bộ mới vào danh sách
                roomTypeInfos.Add((typeName, numberOfRoom, typePrice, discount));
            }
            ViewBag.RoomTypeInfos = roomTypeInfos;

            //service
            List<(string? Service, int? Quantity, double Price)> serviceInfos = new List<(string?, int?, double)>();
            var useServices = _context.UseServices
                                .Where(s => s.OrderId == id)
                                .ToList();
            foreach (var item in useServices)
            {
                string? service = _context.Services
                                .Where(s => s.ServiceId == item.ServiceId)
                                .Select(s => s.ServiceName)
                                .FirstOrDefault();

                int? quantity= item.Quantity;
                double price= (double)_context.Services
                                .Where(s => s.ServiceId == item.ServiceId)
                                .Select(s => s.ServicePrice)
                                .FirstOrDefault();
                serviceInfos.Add((service, quantity, price));
            }
            ViewBag.ServiceInfos = serviceInfos;
            return View(hotelOrder);
        }

        // GET: Customer/Order/Create
        public async Task<IActionResult> Create(OrderModel viewModel)
        {
            //Lấy id đang đăng nhập
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;
            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            else
            {
                ViewBag.UserLoggedId = userId;
            }
            var userApplication = _context.ApplicationUser
                                .Where(u => u.Id == userId)
                                .FirstOrDefault();
            if (userApplication == null)
            {
                return NotFound("Lỗi ttrong database á!");
            }
            ViewBag.Wallet = userApplication.Wallet;
            ///////////////
            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart") ?? new List<RoomCart>();
            double discount = 0;
            double tong = 0;
            foreach (var item in cartItems)
            {
                //Lấy giá của loại phòng
                var typePrice = await _context.RoomTypes
                            .Where(rt => rt.TypeId == item.TypeId)
                            .Select(rt => rt.TypePrice)
                            .FirstOrDefaultAsync();
                //Lấy id giảm giá nếu có
                var promotionId = await _context.RoomPromotions
                            .Where(rp => rp.TypeId == item.TypeId
                                         && rp.StartDate <= item.CheckIn
                                         && rp.EndDate >= item.CheckOut)
                            .Select(rp => rp.PromotionId)
                            .FirstOrDefaultAsync();
                //Tìm discount nếu có
                discount = 0;
                if (promotionId != 0)
                {
                    discount = await _context.Promotions
                        .Where(p => p.PromotionId == promotionId && p.IsActive == true)
                        .Select(p => p.Discount)
                        .FirstOrDefaultAsync() ?? 0;
                }
                //số phòng của loại
                int numberOfRoom = item.Quantity;
                //Tính số ngày ở
                DateOnly checkInDate = (DateOnly)item.CheckIn;
                DateOnly checkOutDate = (DateOnly)item.CheckOut;
                int numberOfDays = checkOutDate.DayNumber - checkInDate.DayNumber + 1;
                //Tính tiền phòng theo loại
                tong += (double)typePrice * numberOfRoom * (1 - (discount / 100)) * numberOfDays;
                ViewBag.CheckIn = item.CheckIn;
                ViewBag.CheckOut = item.CheckOut;
            }
            //tổng tiền phòng + dịch vụ
            tong += Convert.ToDouble(HttpContext.Session.GetString("TotalPrice"));
            Console.WriteLine(tong);
            ViewBag.TongValue = tong;
            
            return View(viewModel);
        }

        // POST: Customer/Order/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFinal(OrderModel viewModel)
        {
            //Lấy ví
            decimal wallet;
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;
            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var role = await _userManager.GetRolesAsync(user);
            if (!role.Contains("Customer"))
            {
                return NotFound("Chỉ có customer mới được order!");
            }
            var userApplication = _context.ApplicationUser
                                .Where(u => u.Id == userId)
                                .FirstOrDefault();
            if (userApplication == null)
            {
                return NotFound("Lỗi ttrong database á!");
            }
            wallet = userApplication.Wallet;
            // kiểm tra số dư hợp lệ
            if (wallet < (decimal)viewModel.HotelOrder.Deposits)
            {
                return RedirectToAction("Index", "VNPay", new { area = "Customer", wallet = wallet, deposits = (decimal)viewModel.HotelOrder.Deposits });
            }
            else
            {
                wallet -= (decimal)viewModel.HotelOrder.Deposits;
                userApplication.Wallet = wallet;
                //Cập nhật tiền trong ví
                await _context.SaveChangesAsync();
            }
            // Lưu HotelOrder vào database trước
            viewModel.HotelOrder.Id = userId;
            _context.HotelOrders.Add(viewModel.HotelOrder);
            _context.SaveChanges();

            // Lấy Id của HotelOrder vừa được lưu
            var orderDetail = new OrderDetail();
            orderDetail.OrderId = viewModel.HotelOrder.OrderId;
            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart") ?? new List<RoomCart>();
            foreach(var item in cartItems)
            {
                // Lưu OrderDetail vào database
                orderDetail.TypeId=item.TypeId;
                orderDetail.NumberOfRoom = item.Quantity;
                _context.OrderDetails.Add(orderDetail);
                _context.SaveChanges();

                //RoomOrder
                for (int i = 0; i < item.Quantity; i++)
                {
                    // Lấy các phòng bị trùng ngày
                    var overlappingRoomIds = (from ro in _context.RoomOrders
                                              join ho in _context.HotelOrders
                                              on ro.OrderId equals ho.OrderId
                                              where (ho.OrderStatus != "cancel") &&
                                                    ((item.CheckIn <= ro.CheckOut && item.CheckIn >= ro.CheckIn) ||
                                                     (item.CheckOut <= ro.CheckOut && item.CheckOut >= ro.CheckIn) ||
                                                     (item.CheckIn <= ro.CheckIn && item.CheckOut >= ro.CheckOut))
                                              select ro.RoomId)
                         .Distinct()
                         .ToList();
                    //lấy phòng hợp lệ đầu tiên
                    var room = _context.Rooms
                                .Where(r => r.TypeId == item.TypeId
                                            && r.RoomStatus == "Available"
                                            && r.IsActive == true
                                            && !overlappingRoomIds.Contains(r.RoomId)
                                            )
                                .OrderBy(r => r.RoomId)
                                .FirstOrDefault();
                    var roomOrder = new RoomOrder(); 
                    if (room != null)
                    {
                        roomOrder.RoomId = room.RoomId;
                    }
                    else
                    {
                        return NotFound("No available room found");
                    }
                    roomOrder.OrderId = viewModel.HotelOrder.OrderId;
                    roomOrder.CheckIn = item.CheckIn;
                    roomOrder.CheckOut = item.CheckOut;
                    _context.RoomOrders.Add(roomOrder);
                    _context.SaveChanges();
                }
            }
            //add Use service
            var useServices = HttpContext.Session.GetObjectFromJson<List<UseService>>("UseServices") ?? new List<UseService>();
            //Lấy OrderId hotelOrder ở trên
            
            foreach (var item in useServices)
            {
                var useService = new UseService();
                useService.OrderId = viewModel.HotelOrder.OrderId;
                useService.RoomId = item.RoomId;
                useService.DateUseService = item.DateUseService;
                useService.Quantity = item.Quantity;
                useService.ServiceId = item.ServiceId;
                useService.Id = userId;

                _context.UseServices.Add(useService);
                _context.SaveChanges();
            }
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Index));
        }

        // GET: Customer/Order/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hotelOrder = await _context.HotelOrders.FindAsync(id);
            if (hotelOrder == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.ApplicationUser, "Id", "Id", hotelOrder.Id);
            return View(hotelOrder);
        }

        // POST: Customer/Order/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,OrderDate,OrderStatus,Deposits,Id")] HotelOrder hotelOrder)
        {
            if (id != hotelOrder.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hotelOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HotelOrderExists(hotelOrder.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.ApplicationUser, "Id", "Id", hotelOrder.Id);
            return View(hotelOrder);
        }

        // GET: Customer/Order/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            //check id
            if (id == null)
            {
                return NotFound();
            }
            //Tìm Hotel Order
            var hotelOrder = await _context.HotelOrders
                .Include(h => h.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (hotelOrder == null)
            {
                return NotFound("Hotel Order not found.");
            }
            //Tìm Order Detail
            var orderDetail = await _context.OrderDetails
                .FirstOrDefaultAsync(o => o.OrderId == hotelOrder.OrderId);
            if (orderDetail == null)
            {
                return NotFound("Order Detail not found.");
            }
            //Tìm Room Order
            var roomOrder = await _context.RoomOrders
                .FirstOrDefaultAsync(r => r.OrderId == hotelOrder.OrderId);
            if (roomOrder == null)
            {
                return NotFound("Room order not found.");
            }
            // bỏ vào chung 1 chỗ
            var viewModel = new OrderModel
            {
                HotelOrder = hotelOrder,
                OrderDetail = orderDetail,
                RoomOrder = roomOrder
            };
            return View(viewModel);
        }

        // POST: Customer/Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //Check đăng nhập
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;
            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var userApplication = _context.ApplicationUser
                                .Where(u => u.Id == userId)
                                .FirstOrDefault();

            var hotelOrder = await _context.HotelOrders.FindAsync(id);
            if (hotelOrder != null)
            {
                var roomOrder = await _context.RoomOrders.Where(o => o.OrderId == hotelOrder.OrderId).ToListAsync();
                // Lấy ngày hiện tại
                DateOnly cancelDate = DateOnly.FromDateTime(DateTime.Today);

                // Kiểm tra nếu ngày hủy >= ngày CheckIn của bất kỳ RoomOrder nào
                foreach (var room in roomOrder)
                {
                    if (cancelDate >= room.CheckIn)
                    {
                        return NotFound("Chỉ có thể hủy trước ngày Check In .");
                    }
                }
                // Chuyển Status về cancel 
                hotelOrder.OrderStatus = "cancel";

                // Tìm tất cả room order
                //var roomOrders = _context.RoomOrders.Where(ro => ro.OrderId == id);

                // Xóa
                //_context.RoomOrders.RemoveRange(roomOrders);
                // Tính tiền hoàn trả
                decimal refundAmount = (decimal)(hotelOrder.Deposits * 0.7);

                // Find the user associated with this order and update their wallet
                if (userApplication != null)
                {
                    userApplication.Wallet += refundAmount;
                }
                // Lưu lại
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HotelOrderExists(int id)
        {
            return _context.HotelOrders.Any(e => e.OrderId == id);
        }
    }
}
