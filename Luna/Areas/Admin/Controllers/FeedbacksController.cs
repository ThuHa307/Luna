using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Luna.Data;
using Luna.Models;

namespace Luna.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FeedbacksController : Controller
    {
        private readonly AppDbContext _context;

        public FeedbacksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Feedbacks
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Feedbacks.Include(f => f.Order).Include(f => f.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Admin/Feedbacks/Details/5
        public async Task<IActionResult> Details(int Order, string Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            
            var feedback = await _context.Feedbacks
                .Include(f => f.Order)
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.Id == Id && m.OrderId== Order);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // GET: Admin/Feedbacks/Create
        public IActionResult Create()
        {
            ViewData["OrderId"] = new SelectList(_context.HotelOrders, "OrderId", "OrderId");
            ViewData["Id"] = new SelectList(_context.ApplicationUser, "Id", "Id");
            return View();
        }

        // POST: Admin/Feedbacks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Message,OrderId,Id")] Feedback feedback)
        {
            
                _context.Add(feedback);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            
            ViewData["OrderId"] = new SelectList(_context.HotelOrders, "OrderId", "OrderId", feedback.OrderId);
            ViewData["Id"] = new SelectList(_context.ApplicationUser, "Id", "Id", feedback.Id);
            return View(feedback);
        }

        // GET: Admin/Feedbacks/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.HotelOrders, "OrderId", "OrderId", feedback.OrderId);
            ViewData["Id"] = new SelectList(_context.ApplicationUser, "Id", "Id", feedback.Id);
            return View(feedback);
        }

        // POST: Admin/Feedbacks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Message,OrderId,Id")] Feedback feedback)
        {
            if (id != feedback.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(feedback);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedbackExists(feedback.Id))
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
            ViewData["OrderId"] = new SelectList(_context.HotelOrders, "OrderId", "OrderId", feedback.OrderId);
            ViewData["Id"] = new SelectList(_context.ApplicationUser, "Id", "Id", feedback.Id);
            return View(feedback);
        }

        // GET: Admin/Feedbacks/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .Include(f => f.Order)
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // POST: Admin/Feedbacks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FeedbackExists(string id)
        {
            return _context.Feedbacks.Any(e => e.Id == id);
        }
    }
}
