using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Luna.Data;
using Luna.Models;
using Microsoft.AspNetCore.Authorization;

namespace Luna.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoomPromotionsController : Controller
    {
        private readonly AppDbContext _context;

        public RoomPromotionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: RoomPromotions
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.RoomPromotions.Include(r => r.Promotion).Include(r => r.Type);
            return View(await appDbContext.ToListAsync());
        }

        // GET: RoomPromotions/Details/5
        public async Task<IActionResult> Details(int? typeID, int? promotionID)
        {
            if (typeID == null && promotionID == null)
            {
                return NotFound();
            }
            Console.WriteLine(typeID);
            Console.WriteLine(promotionID);
            var roomPromotion = await _context.RoomPromotions
                .Include(r => r.Promotion)
                .Include(r => r.Type)
                .FirstOrDefaultAsync(m => m.TypeId == typeID && m.PromotionId == promotionID);
            if (roomPromotion == null)
            {
                return NotFound();
            }

            return View(roomPromotion);
        }

        // GET: RoomPromotions/Create
        public IActionResult Create()
        {
            ViewData["PromotionId"] = new SelectList(_context.Promotions, "PromotionId", "Title");
            ViewData["TypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeName");

            return View();
        }

        // POST: RoomPromotions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PromotionId,TypeId,StartDate,EndDate")] RoomPromotion roomPromotion)
        {
            var existingRecord = _context.RoomPromotions.FirstOrDefault(rp => rp.PromotionId == roomPromotion.PromotionId && rp.TypeId == roomPromotion.TypeId);
            if (existingRecord == null && roomPromotion.StartDate <= roomPromotion.EndDate && roomPromotion.StartDate != null && roomPromotion.EndDate != null)
            {
                _context.Add(roomPromotion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewData["PromotionId"] = new SelectList(_context.Promotions, "PromotionId", "PromotionId", roomPromotion.PromotionId);
                ViewData["TypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeId", roomPromotion.TypeId);
            }

            return View(roomPromotion);
        }


        // GET: RoomPromotions/Edit/5
        public async Task<IActionResult> Edit(int? PromotionId, int? TypeId)
        {
            if (TypeId == null || PromotionId == null)
            {
                return NotFound();
            }

            var roomPromotion = await _context.RoomPromotions
                .FirstOrDefaultAsync(rp => rp.TypeId == TypeId && rp.PromotionId == PromotionId);

            if (roomPromotion == null)
            {
                return NotFound();
            }

            ViewData["PromotionId"] = new SelectList(_context.Promotions, "PromotionId", "PromotionId", roomPromotion.PromotionId);
            ViewData["TypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeId", roomPromotion.TypeId);

            return View(roomPromotion);
        }


        // POST: RoomPromotions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? PromotionId, int? TypeId, [Bind("PromotionId,TypeId,StartDate,EndDate")] RoomPromotion roomPromotion)
        {
            if (TypeId != roomPromotion.TypeId && PromotionId != roomPromotion.PromotionId)
            {
                Console.WriteLine("loi not found");
                return NotFound();
            }
            if (roomPromotion.StartDate < roomPromotion.EndDate && roomPromotion.StartDate != null && roomPromotion.EndDate != null)
            {
                try
                {
                    _context.Update(roomPromotion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomPromotionExists(roomPromotion.TypeId))
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
            ViewData["PromotionId"] = new SelectList(_context.Promotions, "PromotionId", "PromotionId", roomPromotion.PromotionId);
            ViewData["TypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeId", roomPromotion.TypeId);
            return View(roomPromotion);
        }

        // GET: RoomPromotions/Delete/5
        public async Task<IActionResult> Delete(int? promotionId, int? typeId)
        {
            if (typeId == null || promotionId == null)
            {
                return NotFound();
            }

            var roomPromotion = await _context.RoomPromotions
                .Include(r => r.Promotion)
                .Include(r => r.Type)
                .FirstOrDefaultAsync(m => m.TypeId == typeId && m.PromotionId == promotionId);
            if (roomPromotion == null)
            {
                return NotFound();
            }

            return View(roomPromotion);
        }

        // POST: RoomPromotions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? promotionId, int? typeId)
        {
            if (promotionId == null || typeId == null)
            {
                return NotFound();
            }

            var roomPromotion = await _context.RoomPromotions
                .FirstOrDefaultAsync(m => m.PromotionId == promotionId && m.TypeId == typeId);
            if (roomPromotion != null)
            {
                _context.RoomPromotions.Remove(roomPromotion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomPromotionExists(int id)
        {
            return _context.RoomPromotions.Any(e => e.TypeId == id);
        }
    }
}