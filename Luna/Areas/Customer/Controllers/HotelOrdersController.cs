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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Azure;

namespace Luna.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HotelOrdersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        public HotelOrdersController(AppDbContext context
            , UserManager<IdentityUser> userManager,
            IEmailSender emailSender
            )
        {
            _emailSender = emailSender;
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
            List<(string? TypeName, int? NumberOfRoom, double TypePrice, double Discount, int DiscountDays)> roomTypeInfos = new List<(string?, int?, double, double, int)>();

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
                ///////////////
                DateOnly checkInDate = (DateOnly)_context.RoomOrders
                                .Where(ro => ro.OrderId == id && ro.CheckIn.HasValue && ro.CheckOut.HasValue)
                                .Select(ro => ro.CheckIn)
                                .FirstOrDefault();
                DateOnly checkOutDate = (DateOnly)_context.RoomOrders
                                .Where(ro => ro.OrderId == id && ro.CheckIn.HasValue && ro.CheckOut.HasValue)
                                .Select(ro => ro.CheckOut)
                                .FirstOrDefault();
                int numberOfDays = checkOutDate.DayNumber - checkInDate.DayNumber + 1;
                int numberOfDateDiscount = 0;
                double discount = 0;
                //fix tiền
                for (int i = 0; i < numberOfDays; i++)
                {
                    DateOnly dateCheck = checkInDate.AddDays(i);
                    //Lấy id giảm giá nếu có
                    var promotionId = await _context.RoomPromotions
                                .Where(rp => rp.TypeId == item.TypeId
                                        && _context.RoomOrders.Any(ro => ro.OrderId == hotelOrder.OrderId
                                        && rp.StartDate <= dateCheck
                                        && rp.EndDate >= dateCheck))
                                .Select(rp => rp.PromotionId)
                                .FirstOrDefaultAsync();
                    //Tìm discount nếu có
                    
                    if (promotionId != 0)
                    {
                        discount = await _context.Promotions
                            .Where(p => p.PromotionId == promotionId && p.IsActive == true)
                            .Select(p => p.Discount)
                            .FirstOrDefaultAsync() ?? 0;
                        numberOfDateDiscount++;
                    }
                }
                ////////////////
                // Tạo và thêm một bộ mới vào danh sách
                roomTypeInfos.Add((typeName, numberOfRoom, typePrice, discount, numberOfDateDiscount));
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
                //Kiểm tra
                bool check = false;
                int? quantity = item.Quantity;
                for (int i = 0; i < serviceInfos.Count; i++)
                {
                    if (serviceInfos[i].Service == service)
                    {
                        int? currentQuantity = serviceInfos[i].Quantity;
                        int? newQuantity = currentQuantity.HasValue ? currentQuantity + item.Quantity : (int?)null;
                        serviceInfos[i] = (serviceInfos[i].Service, newQuantity, serviceInfos[i].Price);
                        check = true;
                        break;
                    }
                }
                if (!check)
                {
                    double price = (double)_context.Services
                                .Where(s => s.ServiceId == item.ServiceId)
                                .Select(s => s.ServicePrice)
                                .FirstOrDefault();
                    serviceInfos.Add((service, quantity, price));
                }                                   
            }
            ViewBag.ServiceInfos = serviceInfos;
            bool checkcheckout = true;
            foreach (var item in hotelOrder.RoomOrders)
            {
                if(item.ConfirmCheckOut == null)
                {
                    checkcheckout = false;
                }
            }
            // kiem tra xem da check out het phong hay chua, neu check out roi thi moi cho feedback
            if(checkcheckout)
            {

                // neu co feedback thi hashFeedback = true va khong cho feedback
                bool hashFeedback = _context.Feedbacks.Any(f => f.OrderId == id);
                ViewBag.HashFeedback = hashFeedback;
                Console.WriteLine($"check checkout = {checkcheckout}");
            }
            else
            {
                Console.WriteLine(" khong cho feedback");
                ViewBag.HashFeedback = true;
            }


            
            return View(hotelOrder);
        }

        // GET: Customer/Order/Create
        [Authorize(Roles = "Customer")]
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
            List<(string? TypeName, int NumberOfRoom, double TypePrice, string? img, double Discount, int NumDateDiscount)> roomTypeInfos = new List<(string?, int, double, string?, double, int)>();
            List<(string ServiceName, int? Quantity, double ServicePrice, string img)> serviceInfos = new List<(string, int?, double, string)>();
            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart") ?? new List<RoomCart>();
            double discount = 0;
            double tong = 0;
            foreach (var item in cartItems)
            {
                //Lấy tên của loại phòng
                var typeName = await _context.RoomTypes
                            .Where(rt => rt.TypeId == item.TypeId)
                            .Select(rt => rt.TypeName)
                            .FirstOrDefaultAsync();
                //Lấy giá của loại phòng
                var typePrice = await _context.RoomTypes
                            .Where(rt => rt.TypeId == item.TypeId)
                            .Select(rt => rt.TypePrice)
                            .FirstOrDefaultAsync();
                //Lấy ảnh của loại phòng
                var img = await _context.RoomImages
                            .Where(ri => ri.TypeId == item.TypeId)
                            .Select(ri => ri.Link)
                            .FirstOrDefaultAsync();
                
                //Tính số ngày ở
                DateOnly checkInDate = (DateOnly)item.CheckIn;
                DateOnly checkOutDate = (DateOnly)item.CheckOut;
                int numberOfDays = checkOutDate.DayNumber - checkInDate.DayNumber + 1;
                int numberOfDateDiscount = 0;
                //số phòng của loại
                int numberOfRoom = item.Quantity;
                //fix tiền
                for (int i = 0; i< numberOfDays; i++)
                {
                    DateOnly dateCheck = checkInDate.AddDays(i);
                    //Lấy id giảm giá nếu có
                    var promotionId = await _context.RoomPromotions
                                .Where(rp => rp.TypeId == item.TypeId
                                             && rp.StartDate <= dateCheck
                                             && rp.EndDate >= dateCheck)
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

                        numberOfDateDiscount++;
                    }
                    
                    //Tính tiền phòng theo loại
                    tong += (double)typePrice * numberOfRoom * (1 - (discount / 100));
                }
                
                //////
                ViewBag.Stay = numberOfDays;
                ViewBag.CheckIn = item.CheckIn;
                ViewBag.CheckOut = item.CheckOut;
                roomTypeInfos.Add((typeName, numberOfRoom, (double)typePrice,img, discount, numberOfDateDiscount));
                
            }
            ViewBag.TotalRoom = tong;
            //tổng tiền phòng + dịch vụ
            double totalService = Convert.ToDouble(HttpContext.Session.GetString("TotalPrice"));
            tong += totalService;
            ViewBag.TotalService = totalService;
            ViewBag.TongValue = tong;
            
            var useServices = HttpContext.Session.GetObjectFromJson<List<UseService>>("UseServices") ?? new List<UseService>();
            foreach(var item in useServices)
            {
                //Lấy tên service
                var serviceName = await _context.Services
                            .Where(s => s.ServiceId == item.ServiceId)
                            .Select(s => s.ServiceName)
                            .FirstOrDefaultAsync();
                //Kiểm tra
                bool check = false;
                for (int i = 0; i < serviceInfos.Count; i++)
                {
                    if (serviceInfos[i].ServiceName == serviceName)
                    {
                        int? currentQuantity = serviceInfos[i].Quantity;
                        int? newQuantity = currentQuantity.HasValue ? currentQuantity + item.Quantity : (int?)null;
                        serviceInfos[i] = (serviceInfos[i].ServiceName, newQuantity, serviceInfos[i].ServicePrice, serviceInfos[i].img);
                        check = true;
                        break;
                    }
                }
                if (!check)
                {
                    //Lấy giá service
                    var servicePrice = await _context.Services
                                .Where(s => s.ServiceId == item.ServiceId)
                                .Select(s => s.ServicePrice)
                                .FirstOrDefaultAsync();
                    //Lấy ảnh service
                    var img = await _context.Services
                                .Where(s => s.ServiceId == item.ServiceId)
                                .Select(s => s.ServiceImg)
                                .FirstOrDefaultAsync();
                    int? quantity = item.Quantity;
                    if (quantity == null)
                    {
                        quantity = 0;
                    }
                    //Add vào list
                    if (serviceName !=null)
                    {
                        serviceInfos.Add((serviceName, quantity, (double)servicePrice, img));
                    }                   
                }             
            }
            ViewBag.ServiceInfos = serviceInfos;
            ViewBag.RoomInfos = roomTypeInfos;
            return View(viewModel);
        }
        [Authorize(Roles = "Customer")]
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
            int? orderid = HttpContext.Session.GetInt32("OrderId");
            if(orderid != null)
            {
                viewModel.HotelOrder.OrderId = (int)orderid;
                var orderdate = _context.HotelOrders.Where(o => o.OrderId == (int)orderid).Select(o => o.OrderDate).FirstOrDefault();
                var deposits = _context.HotelOrders.Where(o => o.OrderId == (int)orderid).Select(o => o.Deposits).FirstOrDefault();
                viewModel.HotelOrder.OrderDate = orderdate;
                viewModel.HotelOrder.Deposits = viewModel.HotelOrder.Deposits + deposits;
                _context.HotelOrders.Update(viewModel.HotelOrder);
                _context.SaveChanges();                                          
            }
            else
            {               
                _context.HotelOrders.Add(viewModel.HotelOrder);
                _context.SaveChanges();
                // Lấy Id của HotelOrder vừa được lưu
                var orderDetail = new OrderDetail();
                orderDetail.OrderId = viewModel.HotelOrder.OrderId;

                List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart") ?? new List<RoomCart>();
                foreach (var item in cartItems)
                {
                    // Lưu OrderDetail vào database
                    orderDetail.TypeId = item.TypeId;
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

            }

            Console.WriteLine("AAAAAAAAAAA"+ viewModel.HotelOrder.OrderId);
            
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
            var bill = _context.GetBills(viewModel.HotelOrder.OrderId).FirstOrDefault();
            await _emailSender.SendEmailAsync(bill.Email, "Xác nhận đặt phòng",
                        $"<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"background-color:#ffffff;margin:10px 0;max-width:700px\">\r\n    <tbody>\r\n        <tr>\r\n            <td width=\"35\">&nbsp;</td>\r\n            <td width=\"630\" style=\"text-align:center\">\r\n                <a href=\"https://www.heritagehotelnyc.com\" target=\"_blank\" data-saferedirecturl=\"https://www.google.com/url?q=https://www.heritagehotelnyc.com&amp;source=gmail&amp;ust=1717655976367000&amp;usg=AOvVaw1F2xhqDa_4H5cZ9Qcsyhf3\"><img style=\"width: 300px \" src=\"https://scontent.fdad1-2.fna.fbcdn.net/v/t1.15752-9/447883990_1175848956750963_6637060945666689838_n.png?_nc_cat=102&ccb=1-7&_nc_sid=9f807c&_nc_eui2=AeHyMPZjGUoawz3jPLh2bv4WX69ApN4qpqxfr0Ck3iqmrPPwcZ9VcSWhwDZUM_DjSl3mQ7NcqUAbRsHhUT_aeIYO&_nc_ohc=XrYASQxzLUoQ7kNvgFyLHZK&_nc_ht=scontent.fdad1-2.fna&oh=03_Q7cD1QHGR_OO89OGr0LWLxnw-M7QImi40mvrAAWg9AvcmBtvNA&oe=66C58868\" alt=\"The Heritage Hotel New York City - 18 W 25th Street, New York City, NY 10010, USA\" title=\"The Heritage Hotel New York City - 18 W 25th Street, New York City, NY 10010, USA\" class=\"CToWUd\" data-bit=\"iit\"></a>\r\n\r\n            </td>\r\n            <td width=\"35\">&nbsp;</td>\r\n        </tr>\r\n\r\n        <tr>\r\n            <td width=\"35\">&nbsp;</td>\r\n\r\n            <td width=\"630\" style=\"border:3px solid #dddddd\">\r\n                <table width=\"630\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                    <tbody>\r\n                        <tr>\r\n                            <td width=\"25\">&nbsp;</td>\r\n\r\n                            <td width=\"580\">\r\n                                <table width=\"580\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                                    <tbody>\r\n                                        <tr>\r\n                                            <td>&nbsp;</td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td><h1 style=\"font-family:Arial,sans-serif;font-weight:700;text-align:center;text-transform:uppercase;font-size:27px;letter-spacing:2px;border-bottom:1px solid #000000\">Reservation Confirmation</h1></td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td>&nbsp;</td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td><p style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;margin:0\">Dear <strong>{bill.UserName},</strong> </p></td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td>&nbsp;</td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td><p style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;margin:0;line-height:25px\">Thank you for your reservation made through INNsight.com at <strong>Hotel Del Luna</strong> checking in {bill.Checkin}</p></td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td>&nbsp;</td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td>&nbsp;</td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td>\r\n                                                <table width=\"580\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                                                    <tbody>\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\" style=\"padding:10px;background-color:#eee\"><h2 style=\"text-align:center;font-family:Arial,sans-serif;font-size:20px;letter-spacing:0.5px;color:#555;font-weight:500;margin:0\">Confirmation Details</h2></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\">&nbsp;</td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Reservation ID:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\"><strong>{bill.OrderId}</strong></span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Booking Source:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Website</span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Your Name:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">{bill.FullName}</span></td>\r\n                                                        </tr>\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Your Phone:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">{bill.PhoneNumber}</span></td>\r\n                                                        </tr>\r\n                                                        <tr>\r\n                                                            <td width=\"150\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Your Email:</span></td>\r\n                                                            <td width=\"430\"><span style=\"font-family:Arial,sans-serif;font-size:15px;line-height:25px;display:inline-block\"><i><a href=\"mailto:hatran3072003@gmail.com\" style=\"color:#555\" target=\"_blank\">{bill.Email}</a></i></span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\">&nbsp;</td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\" style=\"padding:10px;background-color:#eee\"><h2 style=\"text-align:center;font-family:Arial,sans-serif;font-size:20px;letter-spacing:0.5px;color:#555;font-weight:500;margin:0\">Property Information</h2></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\">&nbsp;</td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Property Name:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\"><strong>Hotel Del Luna</strong></span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td valign=\"top\" width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Address:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\">\r\n                                                                <span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">\r\n                                                                    Ngũ Hành Sơn, Đà Nẵng, Việt Nam\r\n                                                                </span>\r\n                                                            </td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Phone:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\">\r\n                                                                <span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">\r\n                                                                    +1 (212) 645-3990\r\n                                                                </span>\r\n                                                            </td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Email:</span></td>\r\n                                                            <td width=\"430\"><span style=\"font-family:Arial,sans-serif;font-size:15px;line-height:25px;display:inline-block\"><i><a href=\"mailto:hatran3072003@gmail.com\" style=\"color:#555\" target=\"_blank\">hoteldelluna@gmail.com</a></i></span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\">&nbsp;</td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\" style=\"padding:10px;background-color:#eee\"><h2 style=\"text-align:center;font-family:Arial,sans-serif;font-size:20px;letter-spacing:0.5px;color:#555;font-weight:500;margin:0\">Booking Details</h2></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\">&nbsp;</td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Arrival:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">{bill.Checkin}</span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Departure:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">{bill.Checkout}</span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">No. of Rooms:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">{bill.TotalRoom} </span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Total price:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">{String.Format("{0:N0}", bill.Deposits)} ₫</span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\">&nbsp;</td>\r\n                                                        </tr>\r\n\r\n                                                    </tbody>\r\n                                                </table>\r\n                                            </td>\r\n                                        </tr>\r\n\r\n                                    </tbody>\r\n                                </table>\r\n                            </td>\r\n\r\n                            <td width=\"25\">&nbsp;</td>\r\n\r\n                        </tr>\r\n\r\n                    </tbody>\r\n                </table>\r\n\r\n            </td>\r\n\r\n            <td width=\"35\">&nbsp;</td>\r\n\r\n        </tr>\r\n\r\n    </tbody>\r\n</table>");
            HttpContext.Session.Clear();
            HttpContext.Session.SetString("wallet", userApplication.Wallet.ToString());
            TempData["Message"] = $"Booking successful. Please monitor your email or booking history for details.";
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
                        TempData["Message"] = $"Reservation cancellation failed. Cancellations can only be made before Check In date.";
                        return RedirectToAction(nameof(Index));
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
            HttpContext.Session.SetString("wallet", userApplication.Wallet.ToString());
            TempData["Message"] = $"Cancellation of booking successful.";
            return RedirectToAction(nameof(Index));
        }

        private bool HotelOrderExists(int id)
        {
            return _context.HotelOrders.Any(e => e.OrderId == id);
        }
    }
}
