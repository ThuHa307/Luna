using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Luna.Data;
using Luna.Models;
using X.PagedList;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace Luna.Areas.Customer.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoomImageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webhostenvironment;
        public RoomImageController(AppDbContext context, IWebHostEnvironment webhostenvironment)
        {
            _context = context;
            _webhostenvironment = webhostenvironment;
        }

        // GET: Customer/RoomImage
        public async Task<IActionResult> Index(int? page, int? search)
        {
            int pageSize = 3;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var appDbContext = _context.RoomImages.Include(r => r.Type).AsQueryable();
            if (search.HasValue)
            {
                appDbContext = appDbContext.Where(r => r.TypeId == search.Value);
            }
            PagedList<RoomImage> lst = new PagedList<RoomImage>(appDbContext, pageNumber, pageSize);
            ViewData["CurrentSearch"] = search;
            return View(lst);
            //var appDbContext = _context.RoomImages.Include(r => r.Type);
            //var model = await appDbContext.OrderBy(item => item.Type.TypeId).ToListAsync();
            //return View(model);
        }

        // GET: Customer/RoomImage/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomImage = await _context.RoomImages
                .Include(r => r.Type)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomImage == null)
            {
                return NotFound();
            }

            return View(roomImage);
        }

        // GET: Customer/RoomImage/Create
        public IActionResult Create()
        {
            ViewData["TypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeId");
            return View();
        }

        // POST: Customer/RoomImage/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Link,TypeId")] RoomImage roomImage, IFormFile Link)
        {
            if (ModelState.IsValid)
            {
                if (Link != null)
                {
                    // Define the folder path
                    string folder = "img/";
                    // Generate a unique file name
                    string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Link.FileName);
                    // Combine the path with the root path of the application
                    string serverFolder = Path.Combine(_webhostenvironment.WebRootPath, folder, fileName);

                    // Copy the uploaded file to the server
                    using (var fileStream = new FileStream(serverFolder, FileMode.Create))
                    {
                        // Await the copy operation
                        await Link.CopyToAsync(fileStream);
                    }

                    // Update the image path in the roomImage object
                    roomImage.Link = "/" + folder + fileName;
                }
                _context.Add(roomImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeId", roomImage.TypeId);
            return View(roomImage);
        }
       
        // GET: Customer/RoomImage/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomImage = await _context.RoomImages.FindAsync(id);
            if (roomImage == null)
            {
                return NotFound();
            }
            ViewData["TypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeId", roomImage.TypeId);
            return View(roomImage);
        }

        // POST: Customer/RoomImage/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Link,TypeId")] RoomImage roomImage, IFormFile Link)
        {
            if (id != roomImage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (Link != null && Link.Length > 0)
                    {
                        // Define the folder path
                        string folder = "img/";
                        // Generate a unique file name
                        string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Link.FileName);
                        // Combine the path with the root path of the application
                        string serverFolder = Path.Combine(_webhostenvironment.WebRootPath, folder, fileName);

                        // Copy the uploaded file to the server
                        using (var fileStream = new FileStream(serverFolder, FileMode.Create))
                        {
                            await Link.CopyToAsync(fileStream);
                        }

                        // Update the image path in the roomType object
                        roomImage.Link = "/" + folder + fileName;
                    }

                    _context.Update(roomImage);
                    await _context.SaveChangesAsync();
                }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomImageExists(roomImage.Id))
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
        ViewData["TypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeId", roomImage.TypeId);
        return View(roomImage);
        }

        // GET: Customer/RoomImage/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomImage = await _context.RoomImages
                .Include(r => r.Type)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomImage == null)
            {
                return NotFound();
            }

            return View(roomImage);
        }

        // POST: Customer/RoomImage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roomImage = await _context.RoomImages.FindAsync(id);
            if (roomImage != null)
            {
                _context.RoomImages.Remove(roomImage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomImageExists(int id)
        {
            return _context.RoomImages.Any(e => e.Id == id);
        }
    }
}
