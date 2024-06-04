using Luna.Data;
using Luna.Models;
using Luna.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Luna.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderServiceController : Controller
    {
        private OrderDetail GetOrderDetail()
        {
            // Trả về một OrderDetail giả lập
            return new OrderDetail { TypeId = 14, NumberOfRoom = 2 };
        }

        private RoomOrder GetRoomOrder()
        {
            // Trả về một RoomOrder giả lập với DateOnly
            return new RoomOrder { CheckIn = DateOnly.FromDateTime(DateTime.Now), CheckOut = DateOnly.FromDateTime(DateTime.Now.AddDays(3)) };
        }

        private int GetRoomID()
        {
            return 111;
        }
        private readonly AppDbContext _context;

        public OrderServiceController(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Create()
        {
            var orderDetail = GetOrderDetail();
            var roomOrder = GetRoomOrder();


            // Lấy danh sách dịch vụ từ ServicesController
            var services = await _context.Services.ToListAsync();

            var model = new OrderModel
            {
                OrderDetail = orderDetail,
                RoomOrder = roomOrder,
                Services = services
            };

            // Truyền danh sách dịch vụ vào view bằng ViewBag
            ViewBag.Services = services;
            HttpContext.Session.SetInt32("TypeId", orderDetail.TypeId);
            HttpContext.Session.SetInt32("NumberOfRoom", orderDetail.NumberOfRoom.GetValueOrDefault());
            HttpContext.Session.SetObjectAsJson("CheckIn", roomOrder.CheckIn);
            HttpContext.Session.SetObjectAsJson("CheckOut", roomOrder.CheckOut);
            HttpContext.Session.SetInt32("RoomID", roomOrder.RoomId);

            //RoomOrder
            int typeId = model.OrderDetail.TypeId;
            DateOnly checkIn = (DateOnly)model.RoomOrder.CheckIn;
            DateOnly checkOut = (DateOnly)model.RoomOrder.CheckOut;

            List<int> availableRoomIds = new List<int>();

            for (int i = 0; i < model.OrderDetail.NumberOfRoom; i++)
            {
                // Lấy các phòng bị trùng ngày
                var overlappingRoomIds = _context.RoomOrders
                                        .Where(ro =>
                                                    (checkIn <= ro.CheckOut && checkIn >= ro.CheckIn) ||
                                                    (checkOut <= ro.CheckOut && checkOut >= ro.CheckIn) ||
                                                    (checkIn <= ro.CheckIn && checkOut >= ro.CheckOut))
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
                    Console.WriteLine($"RoomID=" + room.RoomId);
                    availableRoomIds.Add(room.RoomId);
                }
                else
                {
                    // Nếu không tìm thấy phòng nào khả dụng, trả về lỗi
                    return NotFound("No available room found");
                }

                Console.WriteLine("Test find RoomID");
                Console.WriteLine(typeId);
                Console.WriteLine(model.RoomOrder.CheckIn);
                Console.WriteLine(model.RoomOrder.CheckOut);
                Console.WriteLine(model.OrderDetail.NumberOfRoom);
            }

            // Tạo danh sách SelectListItem từ danh sách availableRoomIds
            var availableRoomsSelectList = availableRoomIds.Select(id => new SelectListItem
            {
                Value = id.ToString(),
                Text = "Room " + id // Hoặc bất kỳ cách nào bạn muốn hiển thị tên phòng
            }).ToList();

            ViewData["RoomIDs"] = new SelectList(availableRoomsSelectList, "Value", "Text");
            ViewData["DefaultRoomId"] = availableRoomsSelectList.FirstOrDefault()?.Value;

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

            var existingService = useServices.FirstOrDefault(us => us.ServiceId == serviceId && us.DateUseService == date && us.RoomId==roomId);

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
                servicePrice = service.ServicePrice* quantity;
            }

            // Lấy giá trị của totalPrice từ Session và chuyển đổi thành decimal
            var totalPriceString = HttpContext.Session.GetString("TotalPrice");
            decimal totalPriceDecimal = Convert.ToDecimal(totalPriceString);

            // Thực hiện phép tính và gán vào totalPriceDecimal
            decimal totalPrice = totalPriceDecimal+ servicePrice;

            HttpContext.Session.SetString("TotalPrice", totalPrice.ToString());

            return RedirectToAction("CheckSessionData");
        }

        public IActionResult CheckSessionData()
        {
            var typeId = HttpContext.Session.GetInt32("TypeId");
            var numberOfRoom = HttpContext.Session.GetInt32("NumberOfRoom");
            var checkIn = HttpContext.Session.GetObjectFromJson<DateOnly>("CheckIn");
            var checkOut = HttpContext.Session.GetObjectFromJson<DateOnly>("CheckOut");
            var totalPrice = HttpContext.Session.GetString("TotalPrice");

            var useServices = HttpContext.Session.GetObjectFromJson<List<UseService>>("UseServices") ?? new List<UseService>();

            var sessionDataViewModel = new SessionDataViewModel
            {
                TypeId = typeId,
                NumberOfRoom = numberOfRoom,
                CheckIn = checkIn,
                CheckOut = checkOut,
                UseServices = useServices,
                TotalPrice = totalPrice
            };

            Console.WriteLine("Print total Price");
            Console.WriteLine(totalPrice);

            return View(sessionDataViewModel);
        }


        [HttpPost]
        public IActionResult UpdateCart(int serviceId, DateTime date, int roomId, int quantity)
        {
            var useServices = HttpContext.Session.GetObjectFromJson<List<UseService>>("UseServices") ?? new List<UseService>();

            var service = useServices.FirstOrDefault(us =>   us.ServiceId == serviceId && us.DateUseService == date && us.RoomId == roomId);

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

            return RedirectToAction("CheckSessionData");
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
