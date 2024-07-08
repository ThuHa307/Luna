using Azure;
using Luna.Data;
using Luna.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using X.PagedList;

namespace Luna.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class RoomController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RoomController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        //public IActionResult Room()
        //{
        //    List<RoomType> list = _context.RoomTypes.ToList();

        //    return View(list);
        //}
        public IActionResult Room(int? page)
        {
            var userId = _userManager.GetUserId(User);
            int pageSize = 6;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var listSp = _context.RoomTypes.AsNoTracking().OrderBy(x => x.TypePrice);
            var promotions = _context.Promotions.AsNoTracking().ToList();
            var roomPromotion = _context.RoomPromotions.AsNoTracking().ToList();
            
            PagedList<RoomType> lst = new PagedList<RoomType>(listSp, pageNumber, pageSize);
            ViewBag.Promotions = promotions;
            ViewBag.RoomPromotions = roomPromotion;
            if(userId != null)
            {
                var wishlist = _context.WishLists.Where(wl => wl.UserId == userId).ToList();
                if(wishlist.Count > 0)
                {
                    ViewBag.WishList = wishlist;
                }
            }
            var feedbacks = _context.Feedbacks.Include(f => f.User).Where(f => f.Show == true).ToList();
            ViewBag.Feedbacks = feedbacks;
            return View(lst);
        }
        

        // Display the search form
        //public IActionResult SearchRoom()
        //{
        //    return View(new RoomSearchViewModel());
        //}

        //// Handle the search request
        //[HttpPost]
        //public async Task<IActionResult> Search(RoomSearchViewModel searchModel)
        //{
        //    // Implement the search logic here
        //    var searchResults = await _db.Rooms
        //    .ToListAsync();

        //    // Update the search model with the results
        //    searchModel.SearchResults = searchResults;

        //    // Return the view with the updated search model
        //    return View("SearchRoom", searchModel);
        //}

        // Display the search form
        public IActionResult SearchRoom()
        {
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableRooms(DateOnly checkIn, DateOnly checkOut, int? page)
        {
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);

            //if (checkIn < currentDate || checkOut < currentDate)
            //{
            //    ViewData["StatusMessage"] = "Ngày checkin checkout không được nhỏ hơn ngày hiện tại!";
            //    return View("SearchRoom");
            //}
            if (checkIn > checkOut || checkIn < currentDate || checkOut < currentDate)
            {
                ViewData["StatusMessage"] = "Check-in Check-out not available!!";
                return View("SearchRoom");
            }
            var availableRooms = (from a in _context.Rooms
                                  join b in _context.RoomTypes on a.TypeId equals b.TypeId
                                  where a.RoomStatus == "Available" && a.IsActive == true
                            && !_context.RoomOrders.Any(ro => ro.RoomId == a.RoomId &&
                                                              (ro.CheckIn <= checkOut && ro.CheckOut >= checkIn))
                            && !_context.RoomOrders.Any(ro => ro.RoomId == a.RoomId &&
                                                              _context.HotelOrders.Any(ho => ho.OrderId == ro.OrderId && ho.OrderStatus == "cancel"))
                                  group a by new { a.TypeId, b.TypeName, b.TypePrice, b.TypeImg } into g
                                  select new RoomSearchViewModel
                                  {
                                      TypeId = g.Key.TypeId,
                                      TypeName = g.Key.TypeName,
                                      TypePrice = g.Key.TypePrice,
                                      TypeImg = g.Key.TypeImg,
                                      TotalEmpty = g.Count()
                                  }).ToList();

            foreach (var room in availableRooms)
            {
                Console.WriteLine($"TypeId: {room.TypeId}, TypeName: {room.TypeName}, TotalEmpty: {room.TotalEmpty}");
            }

            int pageSize = 6;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            //PagedList<Room> lst = new PagedList<Room>(availableRooms, pageNumber, pageSize);
            var pagedRooms = await availableRooms.ToPagedListAsync(pageNumber, pageSize);
            ViewBag.CheckIn = checkIn;
            ViewBag.CheckOut = checkOut;
            ViewData["checkindate"] = checkIn;
            ViewData["checkoutdate"] = checkOut;
            var roomPromotion = _context.RoomPromotions.AsNoTracking().ToList();
            var promotions = _context.Promotions.AsNoTracking().ToList();
            ViewBag.Promotions = promotions;
            ViewBag.RoomPromotions = roomPromotion;
            return View("SearchRoom", pagedRooms);
            
            //return View("SearchRoom", availableRooms);
        }

        // GET: Rooms
        //Search room + pagatination
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int? page, int? search)
        {
            int pageSize = 3;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            var roomsQuery = _context.Rooms.Include(r => r.Type).AsQueryable();

            if (search.HasValue)
            {
                roomsQuery = roomsQuery.Where(r => r.TypeId == search.Value);
            }

            PagedList<Room> lst = new PagedList<Room>(roomsQuery, pageNumber, pageSize);

            ViewData["CurrentSearch"] = search;

            return View(lst);
        }

        // GET: Rooms/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Type)
                .FirstOrDefaultAsync(m => m.RoomId == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: Rooms/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["TypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeId");
            return View();
        }

        // POST: Rooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,RoomStatus,IsActive,TypeId")] Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeId", room.TypeId);
            return View(room);
        }

        // GET: Rooms/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            ViewData["TypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeId", room.TypeId);
            return View(room);
        }

        // POST: Rooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,RoomStatus,IsActive,TypeId")] Room room)
        {
            if (id != room.RoomId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.RoomId))
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
            ViewData["TypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeId", room.TypeId);
            return View(room);
        }

        // GET: Rooms/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Type)
                .FirstOrDefaultAsync(m => m.RoomId == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }
        [Authorize(Roles = "Admin")]
        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.RoomId == id);
        }
        //chi tiet san pham
        public IActionResult RoomDetail(int maSp, DateTime checkIn, DateTime checkOut)
        {
            var sanPham = _context.RoomTypes.SingleOrDefault(x => x.TypeId == maSp);
            var anhSanpham = _context.RoomImages.Where(x => x.TypeId == maSp).ToList();
            ViewBag.anhSanpham = anhSanpham;

            var otherRoomTypes = _context.RoomTypes.Where(r => r.TypeId != maSp).Take(5).ToList(); // Lấy 3 phòng khác

            // Truyền danh sách phòng khác vào ViewBag
            ViewBag.OtherRoomTypes = otherRoomTypes;

            //Truyền số lượng phòng trống vào ViewBag
            DateOnly checkInDateOnly = DateOnly.FromDateTime(checkIn.Date);
            DateOnly checkOutDateOnly = DateOnly.FromDateTime(checkOut.Date);

            //var availableRoomCount = _context.Rooms
            //    .Where(a => a.TypeId == maSp && a.RoomStatus == "Available" && a.IsActive == false
            //                && !_context.RoomOrders.Any(ro => ro.RoomId == a.RoomId &&
            //                                                  (ro.CheckIn <= checkOutDateOnly && ro.CheckOut >= checkInDateOnly)))
            //    .Count();

            //// Pass the count to the view using ViewBag
            //ViewBag.AvailableRoomCount = availableRoomCount;

            return View(sanPham);
        }
        

        //private List<RoomSearchViewModel> GetAvailableRoom(DateTime checkIn, DateTime checkOut)
        //{
        //    DateOnly checkInDateOnly = DateOnly.FromDateTime(checkIn.Date);
        //    DateOnly checkOutDateOnly = DateOnly.FromDateTime(checkOut.Date);

        //    var availableRooms = (from a in _context.Rooms
        //                          join b in _context.RoomTypes on a.TypeId equals b.TypeId
        //                          where a.RoomStatus == "Available" && a.IsActive == false
        //                                && !_context.RoomOrders.Any(ro => ro.RoomId == a.RoomId &&
        //                                                                  (ro.CheckIn <= checkOutDateOnly && ro.CheckOut >= checkInDateOnly))
        //                          group a by new { a.TypeId, b.TypeName, b.TypePrice, b.TypeImg } into g
        //                          select new RoomSearchViewModel
        //                          {
        //                              TypeId = g.Key.TypeId,
        //                              TypeName = g.Key.TypeName,
        //                              TypePrice = g.Key.TypePrice,
        //                              TypeImg = g.Key.TypeImg,
        //                              TotalEmpty = g.Count()
        //                          }).ToList();

        //    return availableRooms;
        //}
    }

}
