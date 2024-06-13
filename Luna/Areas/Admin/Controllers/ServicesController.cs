﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Luna.Data;
using Luna.Models;
using System.Drawing.Printing;

namespace Luna.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ServicesController : Controller
    {
        private readonly AppDbContext _context;
        //private const int PageSize = 4;

        public ServicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Services
        public async Task<IActionResult> Index(int pageNumber = 1)
        {
   //         var services = await _context.Services
   //         .OrderBy(s => s.ServiceId)
   //             .Skip((pageNumber - 1) * PageSize)
   //             .Take(PageSize)
   //             .ToListAsync();
			//ViewBag.PageSize = PageSize;
            return View();
        }

        // GET: Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // GET: Services/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Services/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceId,ServiceName,ServicePrice,IsActive,ServiceImg,Description")] Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // GET: Services/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        // POST: Services/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceId,ServiceName,ServicePrice,IsActive,ServiceImg,Description")] Service service)
        {
            if (id != service.ServiceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ServiceId))
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
            return View(service);
        }

        // GET: Services/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return View("Index", await _context.Services.ToListAsync());
            }

            var services = await _context.Services
                .Where(s => s.ServiceName.Contains(query) || s.Description.Contains(query))
                .ToListAsync();

            return View("Index", services);
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServiceId == id);
        }
    }
}
//@*     < div class= "slider" >
//        @foreach(var item in promotions)
//        {
//    if (item.IsActive == true)
//    {
//        @foreach(var rp in roomPromotions.Where(rp => rp.PromotionId == item.PromotionId))
//                {
//            var roomName = Model.Where(rt => rt.TypeId == rp.TypeId)
//                    .Select(rt => rt.TypeName)
//                    .FirstOrDefault() ?? "Không xác định";
//                    < p class= "slide" >🌟 Khuyến Mãi Đặc Biệt! 🌟<span>@item.Discount%</span> cho loại phòng <span>@roomName</span> từ ngày <span>@rp.StartDate</span> tới ngày <span>@rp.EndDate</span></p>
//                }            
//            }
//        }
//    </ div > *@
//var promotions = ViewBag.Promotions as List<Luna.Models.Promotion>;
//var roomPromotions = ViewBag.RoomPromotions as List<Luna.Models.RoomPromotion>;