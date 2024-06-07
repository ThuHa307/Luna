using Luna.Areas.Customer.Models;
using Luna.Data;
using Luna.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Luna.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        public CartController(AppDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Add(int typeid, int quantityInput, string checkindate, string checkoutdate)
        {
            Console.WriteLine("So luongaaaaaaaaaaaaaaaaaaaaaaa: " + quantityInput);
            HttpContext.Session.SetInt32("quantity", quantityInput);

            RoomType room = await _context.RoomTypes.FindAsync(typeid);

            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart") ?? new List<RoomCart>();

            // Parse check-in and check-out dates
            DateOnly checkInDate = DateOnly.Parse(checkindate);
            DateOnly checkOutDate = DateOnly.Parse(checkoutdate);

            // Query to get the number of available rooms for the given dates and typeid
            var availableRoomsCount = (from a in _context.Rooms
                                       where a.TypeId == typeid && a.RoomStatus == "Available" && a.IsActive == true
                                             && !_context.RoomOrders.Any(ro => ro.RoomId == a.RoomId &&
                                                                               (ro.CheckIn <= checkOutDate && ro.CheckOut >= checkInDate))
                                       select a).Count();

            if (availableRoomsCount < quantityInput)
            {
                quantityInput = availableRoomsCount;
                TempData["WarningMessage"] = $"Only {availableRoomsCount} rooms are available for the selected dates. The quantity has been adjusted.";
            }

            // Find the cart item with the same typeId, check-in, and check-out dates
            RoomCart cartItem = cartItems.FirstOrDefault(c => c.TypeId == typeid && c.CheckIn == checkInDate && c.CheckOut == checkOutDate);

            if (cartItem == null)
            {
                cartItems.Add(new RoomCart(room, quantityInput, checkInDate, checkOutDate));
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
                    TempData["WarningMessage"] = $"Only {availableRoomsCount} rooms are available for the selected dates. The quantity has been adjusted.";
                }
            }

            HttpContext.Session.SetJson("Cart", cartItems);
            return Redirect(Request.Headers["Referer"].ToString());
        }
        public async Task<IActionResult> DecreaseSL(int Id)
        {
            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart");
            RoomCart cartItem = cartItems.Where(c => c.TypeId == Id).FirstOrDefault();
            if(cartItem.Quantity > 1)
            {
                --cartItem.Quantity;
            }
            else
            {
                cartItems.RemoveAll(p => p.TypeId == Id);
            }
            if(cartItems.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cartItems);
            }
            return RedirectToAction("Index");
        }
        //public async Task<IActionResult> IncreaseSL(int Id)
        //{
        //    List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart");
        //    RoomCart cartItem = cartItems.Where(c => c.TypeId == Id).FirstOrDefault();
        //    if (cartItem.Quantity >= 1 && ) 
        //    {
        //        ++cartItem.Quantity;
        //    }
        //    else
        //    {
        //        cartItems.RemoveAll(p => p.TypeId == Id);
        //    }
        //    if (cartItems.Count == 0)
        //    {
        //        HttpContext.Session.Remove("Cart");
        //    }
        //    else
        //    {
        //        HttpContext.Session.SetJson("Cart", cartItems);
        //    }
        //    return RedirectToAction("Index");
        //}
        public async Task<IActionResult> IncreaseSL(int Id, DateOnly checkIn, DateOnly checkOut)
        {
            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart");
            RoomCart cartItem = cartItems.Where(c => c.TypeId == Id).FirstOrDefault();

            if (cartItem != null)
            {
                var availableRoomsCount = (from a in _context.Rooms
                                           where a.TypeId == Id && a.RoomStatus == "Available" && a.IsActive == true
                                                 && !_context.RoomOrders.Any(ro => ro.RoomId == a.RoomId &&
                                                                                   (ro.CheckIn <= checkOut && ro.CheckOut >= checkIn))
                                           select a).Count();

                if (cartItem.Quantity < availableRoomsCount)
                {
                    ++cartItem.Quantity;
                }
                else
                {
                    TempData["ErrorMessage"] = "Cannot increase quantity. No more rooms available.";
                }
            }
            else
            {
                cartItems.RemoveAll(p => p.TypeId == Id);
            }

            if (cartItems.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cartItems);
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Remove(int Id)
        {
            List<RoomCart> cartItems = HttpContext.Session.GetJson<List<RoomCart>>("Cart");
            cartItems.RemoveAll(p => p.TypeId == Id);
            if(cartItems.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cartItems);
            }
            return RedirectToAction("Index");
        }
    }
}
