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
using Luna.Services;


namespace Luna.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class FeedbacksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SentimentAnalysisService _sentimentAnalysisService;

        public FeedbacksController(AppDbContext context, UserManager<IdentityUser> userManager, SentimentAnalysisService sentimentAnalysisService)
        {
            _context = context;
            _userManager = userManager;
            _sentimentAnalysisService = sentimentAnalysisService;

        }

        // GET: Customer/Feedbacks
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Feedbacks.Include(f => f.Order).Include(f => f.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Customer/Feedbacks/Details/5
        public async Task<IActionResult> Details(string id)
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

        // GET: Customer/Feedbacks/Create
        public async Task<IActionResult> Create(int orderID)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;
            ViewData["OrderId"] = orderID;
            ViewData["Id"] = userId;
            return View();
        }

        // POST: Customer/Feedbacks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Message,OrderId,Id,Show")] Feedback feedback)
        //{

        //        _context.Add(feedback);
        //        await _context.SaveChangesAsync();
        //        //return RedirectToAction(nameof(Index));

        //    //ViewData["OrderId"] = new SelectList(_context.HotelOrders, "OrderId", "OrderId", feedback.OrderId);
        //    //ViewData["Id"] = new SelectList(_context.ApplicationUser, "Id", "Id", feedback.Id);
        //    return RedirectToAction("Details", "HotelOrders", new { area = "Customer" , id = feedback.OrderId});
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Message,OrderId,Id,Show")] Feedback feedback)
        {
            
                // Phân tích cảm xúc của feedback
                var isPositive = _sentimentAnalysisService.PredictSentiment(feedback.Message);
                feedback.Show = isPositive; // sua thuoc tinh cua feedback neu no tot = true/ xau = false hehe

                _context.Add(feedback);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "HotelOrders", new { area = "Customer", id = feedback.OrderId });
            
            return View(feedback);
        }

        // GET: Customer/Feedbacks/Edit/5
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

        // POST: Customer/Feedbacks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Message,OrderId,Id,Show")] Feedback feedback)
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

        // GET: Customer/Feedbacks/Delete/5
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

        // POST: Customer/Feedbacks/Delete/5
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
