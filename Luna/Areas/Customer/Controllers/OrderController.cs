using Luna.Areas.Customer.Models;
using Luna.Data;
using Luna.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Luna.Utility;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Luna.Areas.Customer.Controllers
{
    [Area("Customer")]

    public class OrderController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;
        public OrderController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart") ?? new List<RoomCart>();
            BookingCart cartBook = new()
            {
                items = cartItems,
                totalPrice = cartItems.Sum(x => x.Quantity * x.TypePrice),
            };
            return View(cartBook);
        }
        public IActionResult CheckOut()
        {
            return View();
        }
        //public async Task<IActionResult> Add(int typeid, int quantityInput, DateOnly checkInDate, DateOnly checkOutDate)
        //{
        //    // Use the received data as needed
        //    RoomType room = await _context.RoomTypes.FindAsync(typeid);
        //    List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart") ?? new List<RoomCart>();
        //    RoomCart cartItem = cartItems.FirstOrDefault(c => c.TypeId == typeid);

        //    if (cartItem == null)
        //    {
        //        cartItems.Add(new RoomCart(room, quantityInput, checkInDate, checkOutDate));
        //    }
        //    else
        //    {
        //        cartItem.Quantity += 1;
        //    }

        //    HttpContext.Session.SetJson("Cart", cartItems);
        //    return RedirectToAction("Index"); // Redirect to appropriate action after processing
        //}
        public async Task<IActionResult> Add(int typeid, int quantityInput, string checkindate, string checkoutdate, decimal typePrice)
        {
            Console.WriteLine("So luongaaaaaaaaaaaaaaaaaaaaaaa: " + quantityInput);
            HttpContext.Session.SetInt32("quantity", quantityInput);
            typePrice = typePrice;
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"Price: {typePrice}");
            RoomType room = await _context.RoomTypes.FindAsync(typeid);

            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart") ?? new List<RoomCart>();

            DateOnly checkInDate = DateOnly.Parse(checkindate);
            DateOnly checkOutDate = DateOnly.Parse(checkoutdate);

            //phòng trong cart
            foreach (var item in cartItems)
            {
                if (item.CheckIn != checkInDate || item.CheckOut != checkOutDate)
                {
                    //ViewData["StatusMessage"] = "Check-in Check-out not available in cart!";
                    // return View("SearchRoom");
                    return RedirectToAction("GetAvailableRooms", "Room", new { area = "Admin" });
                };
            }
            // Query to get the number of available rooms for the given dates and typeid
            var availableRoomsCount = (from a in _context.Rooms
                                       where a.TypeId == typeid && a.RoomStatus == "Available" && a.IsActive == true
                                             && !_context.RoomOrders.Any(ro => ro.RoomId == a.RoomId &&
                                                                               (ro.CheckIn <= checkOutDate && ro.CheckOut >= checkInDate))
                                              && !_context.RoomOrders.Any(ro => ro.RoomId == a.RoomId &&
                                                              _context.HotelOrders.Any(ho => ho.OrderId == ro.OrderId && ho.OrderStatus == "cancel"))
                                       select a).Count();

            if (availableRoomsCount < quantityInput)
            {
                quantityInput = availableRoomsCount;
            }

            // Find the cart item with the same typeId, check-in, and check-out dates
            RoomCart cartItem = cartItems.FirstOrDefault(c => c.TypeId == typeid && c.CheckIn == checkInDate && c.CheckOut == checkOutDate /*&& c.TypePrice== typePrice*/);

            if (cartItem == null)
            {
                cartItems.Add(new RoomCart(room, quantityInput, checkInDate, checkOutDate, typePrice));
            }
            else
            {
                if (cartItem.Quantity + quantityInput <= availableRoomsCount)
                {
                    cartItem.Quantity += quantityInput;
                }
                else
                {
                    cartItem.Quantity = availableRoomsCount;
                }
            }
            Console.WriteLine("Check-in: " + checkInDate);
            Console.WriteLine("Check-in: " + checkOutDate);
            Console.WriteLine("aaaaa: " + availableRoomsCount);
            HttpContext.Session.SetJson("Cart", cartItems);
            HttpContext.Session.SetJson("Count", cartItems.Sum(c => c.Quantity));
            HttpContext.Session.SetString("CheckInDate", checkInDate.ToString("yyyy-MM-dd"));
            HttpContext.Session.SetString("CheckOutDate", checkOutDate.ToString("yyyy-MM-dd"));
            Console.WriteLine("Count" + cartItems.Sum(c => c.Quantity));
            return Redirect(Request.Headers["Referer"].ToString());
            // TempData["AlertMessage"] = "Item added to cart successfully!";

            //  return Redirect(Request.Headers["Referer"].ToString());


        }
        //public int GetCartCount()
        //{
        //    List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart") ?? new List<RoomCart>();
        //    int cartCount = cartItems.Sum(c => c.Quantity);
        //    return Json(cartCount);
        //}
        public async Task<IActionResult> DecreaseSL(int Id)
        {
            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart");
            RoomCart cartItem = cartItems.Where(c => c.TypeId == Id).FirstOrDefault();
            if (cartItem.Quantity > 1)
            {
                --cartItem.Quantity;
            }
            else
            {
                cartItems.RemoveAll(p => p.TypeId == Id);
            }
            if (cartItems.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
                HttpContext.Session.Remove("Count");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cartItems);
                HttpContext.Session.SetJson("Count", cartItems.Sum(c => c.Quantity));
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> IncreaseSL(int Id)
        {
            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart");
            RoomCart cartItem = cartItems.Where(c => c.TypeId == Id).FirstOrDefault();
            DateOnly checkIn = DateOnly.Parse(HttpContext.Session.GetString("CheckInDate"));
            DateOnly checkOut = DateOnly.Parse(HttpContext.Session.GetString("CheckOutDate"));
            if (cartItem != null)
            {
                var availableRoomsCount = await _context.Rooms
                                          .Where(a => a.TypeId == Id && a.RoomStatus == "Available" && a.IsActive == true
                                                  && !_context.RoomOrders.Any(ro => ro.RoomId == a.RoomId &&
                                                                                    (ro.CheckIn <= checkOut && ro.CheckOut >= checkIn))
                                                  && !_context.RoomOrders.Any(ro => ro.RoomId == a.RoomId &&
                                                                                    _context.HotelOrders.Any(ho => ho.OrderId == ro.OrderId && ho.OrderStatus == "cancel")))
                                          .Select(a => a.RoomId)
                                          .Distinct()
                                          .CountAsync();
                Console.WriteLine("Check-in: " + checkIn);
                Console.WriteLine("Check-in: " + checkOut);
                Console.WriteLine("aaaaa: " + availableRoomsCount);
                if (cartItem.Quantity < availableRoomsCount)
                {
                    ++cartItem.Quantity;
                }
                else
                {
                    //TempData["ErrorMessage"] = "Cannot increase quantity. No more rooms available.";
                }
            }
            else
            {
                cartItems.RemoveAll(p => p.TypeId == Id);
            }

            if (cartItems.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
                HttpContext.Session.Remove("Count");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cartItems);
                HttpContext.Session.SetJson("Count", cartItems.Sum(c => c.Quantity));
            }

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Remove(int Id, string check_in, string check_out)
        {
            DateOnly checkInDate = DateOnly.Parse(check_in);
            DateOnly checkOutDate = DateOnly.Parse(check_out);
            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart");
            cartItems.RemoveAll(p => p.TypeId == Id && p.CheckIn == checkInDate && p.CheckOut == checkOutDate);
            if (cartItems.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
                HttpContext.Session.Remove("Count");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cartItems);
                HttpContext.Session.SetJson("Count", cartItems.Sum(c => c.Quantity));
            }
            return RedirectToAction("Index");
        }
        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////////////Service//////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            Console.WriteLine("user=%s", user?.UserName);
            var userId = user?.Id;
            Console.WriteLine("userId=%s", userId);

            // Lấy danh sách dịch vụ từ ServicesController
            var services = await _context.Services.ToListAsync();
            var roomType = await _context.RoomTypes.ToListAsync();
            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart") ?? new List<RoomCart>();
            var availableRoomsByTypeId = new Dictionary<int, List<int>>();
            List<int> typeIds = new List<int>();
            //if (cartItems.Count == 0)
            //{
            //    return NotFound("Cart is empty");
            //}
            int typeId;
            int numberOfRoom;
            DateOnly? checkIn;
            DateOnly? checkOut;
            var model = new OrderModel
            {
                OrderDetail = new OrderDetail(),
                CartItems = cartItems,
                Services = services,
                RoomTypes = roomType
            };
            //no login and no order room
            if (userId == null && cartItems.Count == 0)
            {
                return Redirect("/Admin/Room/Room");
            }

            if (userId != null)
            {
                var currentDateOnly = DateOnly.FromDateTime(DateTime.Now);
                var userHotelOrders = _context.HotelOrders
                                            .Where(ro => ro.Id == userId
                                                        && ro.OrderStatus == "ordered")
                                            .Select(ro => ro.OrderId)
                                            .ToList();
                // Tạo danh sách để lưu các thông tin CheckIn, CheckOut và RoomId cho mỗi OrderId
                var orderDetails = new List<dynamic>();

                // Duyệt qua từng OrderId và lấy các RoomOrder tương ứng
                foreach (var orderId in userHotelOrders)
                {
                    var roomOrders = _context.RoomOrders
                                        .Where(ro => ro.OrderId == orderId && ro.CheckIn <= currentDateOnly && ro.CheckOut >= currentDateOnly)
                                        .Select(ro => new
                                        {
                                            ro.RoomId,
                                            ro.CheckIn,
                                            ro.CheckOut,
                                            ro.OrderId,
                                            TypeId = _context.Rooms
                                                .Where(r => r.RoomId == ro.RoomId)
                                                .Select(r => r.TypeId)
                                                .FirstOrDefault()
                                        })
                                        .Distinct()
                                        .ToList();


                    // Lưu trữ thông tin CheckIn, CheckOut và RoomId vào danh sách
                    orderDetails.AddRange(roomOrders);
                }


                if (orderDetails.Count > 0 && cartItems.Count == 0)
                {

                    foreach (var ro in orderDetails)
                    {
                        int orderId = ro.OrderId;
                        HttpContext.Session.SetInt32("OrderId", orderId);

                        //Console.WriteLine("AAAA"+ro.OrderId);
                        typeId = ro.TypeId;
                        if (!typeIds.Contains(typeId))
                        {
                            typeIds.Add(typeId);
                        }
                        checkIn = ro.CheckIn;
                        checkOut = ro.CheckOut;

                        if (!availableRoomsByTypeId.ContainsKey(typeId))
                        {
                            availableRoomsByTypeId[typeId] = new List<int>();
                        }
                        if (!availableRoomsByTypeId[typeId].Contains(ro.RoomId))
                        {
                            availableRoomsByTypeId[typeId].Add(ro.RoomId);
                        }
                    }
                    //ViewBag.MinDate = orderDetails[0].CheckIn.Value.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd") + "T00:00";
                    //ViewBag.MaxDate = orderDetails[0].CheckOut.Value.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd") + "T23:59";
                    ViewBag.Services = services;
                    // Tạo danh sách SelectListItem từ danh sách availableRoomIds
                    ViewBag.AvailableRoomsByTypeId = availableRoomsByTypeId;
                    ViewBag.TypeIds = typeIds;
                    Console.WriteLine("Room Orders for User's Hotel Orders:");
                    foreach (var detail in orderDetails)
                    {
                        Console.WriteLine($" RoomID: {detail.RoomId}, CheckIn: {detail.CheckIn}, CheckOut: {detail.CheckOut}");
                    }
                    return View(model);

                }
                if (orderDetails.Count == 0 && cartItems.Count == 0)
                {
                    return Redirect("/Admin/Room/Room");
                }
            }

            foreach (var cartItem in cartItems)
            {
                typeId = cartItem.TypeId;
                numberOfRoom = cartItem.Quantity;
                checkIn = cartItem.CheckIn;
                checkOut = cartItem.CheckOut;

                typeIds.Add(typeId);


                if (!availableRoomsByTypeId.ContainsKey(typeId))
                {
                    availableRoomsByTypeId[typeId] = new List<int>();
                }

                // Lặp qua số lượng phòng cần tìm
                for (int i = 0; i < numberOfRoom; i++)
                {
                    // Lấy các phòng bị trùng ngày
                    var overlappingRoomIds = _context.RoomOrders
                                            .Where(ro =>
                                                        checkIn <= ro.CheckOut && checkIn >= ro.CheckIn ||
                                                        checkOut <= ro.CheckOut && checkOut >= ro.CheckIn ||
                                                        checkIn <= ro.CheckIn && checkOut >= ro.CheckOut)
                                            .Select(ro => ro.RoomId)
                                            .Distinct()
                                            .ToList();

                    var room = _context.Rooms
                                .Where(r => r.TypeId == typeId
                                            && r.RoomStatus == "Available"
                                            && r.IsActive == true
                                            && !overlappingRoomIds.Contains(r.RoomId))
                                .OrderBy(r => r.RoomId).Skip(i)
                                .FirstOrDefault();

                    if (room != null)
                    {
                        Console.WriteLine("Test find RoomID");
                        Console.WriteLine($"RoomID=" + room.RoomId);
                        availableRoomsByTypeId[typeId].Add(room.RoomId);

                    }
                    else
                    {
                        // Nếu không tìm thấy phòng nào khả dụng, trả về lỗi
                        return NotFound("No available room found");
                    }

                    Console.WriteLine(typeId);
                    Console.WriteLine(checkIn);
                    Console.WriteLine(checkOut);
                    Console.WriteLine(numberOfRoom);
                }
            }
            ViewBag.MinDate = cartItems.FirstOrDefault().CheckIn.Value.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd") + "T00:00";
            ViewBag.MaxDate = cartItems.FirstOrDefault().CheckOut.Value.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd") + "T23:59";

            // Truyền danh sách dịch vụ vào view bằng ViewBag
            ViewBag.Services = services;

            // Tạo danh sách SelectListItem từ danh sách availableRoomIds
            ViewBag.AvailableRoomsByTypeId = availableRoomsByTypeId;

            ViewBag.TypeIds = typeIds;
            //ViewBag.DateInfoByTypeId = dateInfoByTypeId;
            return View(model);
        }

        [HttpPost]
        public IActionResult Create(OrderModel model)
        {
            if (ModelState.IsValid)
            {
                //return View("CheckSessionData");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult AddToCart(int quantity, DateTime date, int serviceId, int roomId, string userId)
        {
            var useService = new UseService
            {
                DateUseService = date,
                Quantity = quantity,
                ServiceId = serviceId,
                RoomId = roomId,
                Id = userId,
            };

            var useServices = HttpContext.Session.GetObjectFromJson<List<UseService>>("UseServices") ?? new List<UseService>();

            var existingService = useServices.FirstOrDefault(us => us.ServiceId == serviceId && us.DateUseService == date && us.RoomId == roomId);

            if (existingService != null)
            {
                existingService.Quantity += quantity;
            }
            else
            {
                useServices.Add(useService);
            }

            HttpContext.Session.SetObjectAsJson("UseServices", useServices);

            decimal servicePrice = 0;

            var service = _context.Services.FirstOrDefault(s => s.ServiceId == serviceId);

            if (service != null)
            {
                servicePrice = service.ServicePrice * quantity;
            }

            // Lấy giá trị của totalPrice từ Session và chuyển đổi thành decimal
            var totalPriceString = HttpContext.Session.GetString("TotalPrice");
            decimal totalPriceDecimal = Convert.ToDecimal(totalPriceString);

            // Thực hiện phép tính và gán vào totalPriceDecimal
            decimal totalPrice = totalPriceDecimal + servicePrice;
            ViewBag.servicePrice = servicePrice;
            HttpContext.Session.SetString("TotalPrice", totalPrice.ToString());

            return Redirect(Request.Headers["Referer"].ToString());

        }

        public IActionResult CheckSessionData()
        {
            var services = _context.Services.ToList();


            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart") ?? new List<RoomCart>();
            //foreach (var cartItem in cartItems)
            //{
            //    Console.WriteLine(cartItem.TypeId);
            //    Console.WriteLine(cartItem.Quantity);
            //    Console.WriteLine(cartItem.CheckIn);
            //    Console.WriteLine(cartItem.CheckOut);
            //}
            var totalPrice = HttpContext.Session.GetString("TotalPrice");

            var useServices = HttpContext.Session.GetObjectFromJson<List<UseService>>("UseServices") ?? new List<UseService>();

            var sessionDataViewModel = new SessionDataViewModel
            {
                UseServices = useServices,
                TotalPrice = totalPrice,
                Services = services
            };

            Console.WriteLine("Print total Price");
            Console.WriteLine(totalPrice);

            return View(sessionDataViewModel);
        }



        [HttpPost]
        public IActionResult UpdateCart(int serviceId, DateTime date, int roomId, int quantity)
        {
            var useServices = HttpContext.Session.GetObjectFromJson<List<UseService>>("UseServices") ?? new List<UseService>();

            var service = useServices.FirstOrDefault(us => us.ServiceId == serviceId && us.DateUseService == date && us.RoomId == roomId);

            if (service != null)
            {
                service.Quantity += quantity;
                if (service.Quantity <= 0)
                {
                    useServices.Remove(service);
                }
                HttpContext.Session.SetObjectAsJson("UseServices", useServices);
            }

            // Recalculate the total price
            decimal? totalPrice = 0;
            foreach (var item in useServices)
            {
                var serviceItem = _context.Services.FirstOrDefault(s => s.ServiceId == item.ServiceId);
                if (serviceItem != null)
                {
                    totalPrice += serviceItem.ServicePrice * item.Quantity;
                }
            }

            HttpContext.Session.SetString("TotalPrice", totalPrice.ToString());
            Console.WriteLine("check update");
            //return RedirectToAction("CheckSessionData");
            return Redirect(Request.Headers["Referer"].ToString());

        }

        public IActionResult RemoveFromCart(int serviceId, DateTime date, int roomId)
        {
            var useServices = HttpContext.Session.GetObjectFromJson<List<UseService>>("UseServices") ?? new List<UseService>();

            var service = useServices.FirstOrDefault(us => us.ServiceId == serviceId && us.DateUseService == date && us.RoomId == roomId);

            if (service != null)
            {
                useServices.Remove(service);
                HttpContext.Session.SetObjectAsJson("UseServices", useServices);
            }

            // Recalculate the total price
            decimal? totalPrice = 0;
            foreach (var item in useServices)
            {
                var serviceItem = _context.Services.FirstOrDefault(s => s.ServiceId == item.ServiceId);
                if (serviceItem != null)
                {
                    totalPrice += serviceItem.ServicePrice * item.Quantity;
                }
            }

            HttpContext.Session.SetString("TotalPrice", totalPrice.ToString());

            return RedirectToAction("CheckSessionData");
        }

    }
}