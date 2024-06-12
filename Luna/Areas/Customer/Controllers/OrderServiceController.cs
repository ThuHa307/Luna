using Luna.Areas.Customer.Models;
using Luna.Data;
using Luna.Models;
using Luna.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Luna.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderServiceController : Controller
    {

        private readonly AppDbContext _context;

        public OrderServiceController(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Create()
        {

            // Lấy danh sách dịch vụ từ ServicesController
            var services = await _context.Services.ToListAsync();
            var roomType = await _context.RoomTypes.ToListAsync();
            var dateInfoByTypeId = new Dictionary<int, (DateTime CheckIn, DateTime CheckOut)>();
            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart") ?? new List<RoomCart>();
            var availableRoomsByTypeId = new Dictionary<int, List<int>>();
            List<int> typeIds = new List<int>();
            if (cartItems.Count == 0)
            {
                return NotFound("Cart is empty");
            }
            // Truyền danh sách dịch vụ vào view bằng ViewBag

            var model = new OrderModel
            {
                OrderDetail = new OrderDetail(),
                CartItems = cartItems,
                Services = services,
                RoomTypes = roomType
            };

            //RoomOrder
            foreach (var cartItem in cartItems)
            {
                int typeId = cartItem.TypeId;
                int numberOfRoom = cartItem.Quantity;
                DateOnly? checkIn = cartItem.CheckIn;
                DateOnly? checkOut = cartItem.CheckOut;
                typeIds.Add(typeId);
                if (cartItem.CheckIn.HasValue && cartItem.CheckOut.HasValue)// lưu lis checkin checkout theo typeID
                {
                    dateInfoByTypeId[cartItem.TypeId] = (cartItem.CheckIn.Value.ToDateTime(TimeOnly.MinValue), cartItem.CheckOut.Value.ToDateTime(TimeOnly.MinValue));
                }

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

            // Truyền danh sách dịch vụ vào view bằng ViewBag
            ViewBag.Services = services;
            // Tạo danh sách SelectListItem từ danh sách availableRoomIds
            ViewBag.AvailableRoomsByTypeId = availableRoomsByTypeId;
            ViewBag.TypeIds = typeIds;
            //Console.WriteLine("TypeIds in ViewBag:");
            //foreach (var typeId in ViewBag.TypeIds)
            //{
            //    Console.WriteLine(typeId);
            //}
            ViewBag.DateInfoByTypeId = dateInfoByTypeId;
            return View(model);

        }

        [HttpPost]
        public IActionResult Create(OrderModel model)
        {
            if (ModelState.IsValid)
            {
                // Lưu model vào database hoặc thực hiện các thao tác cần thiết
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
