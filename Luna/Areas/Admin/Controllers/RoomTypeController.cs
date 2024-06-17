using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Luna.Data;
using Luna.Models;
using Azure;
using X.PagedList;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace Luna.Areas.Customer.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoomTypeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webhostenvironment;

        public RoomTypeController(AppDbContext context, IWebHostEnvironment webhostenvironment)
        {
            _context = context;
            _webhostenvironment = webhostenvironment;
        }

        // GET: Customer/RoomType
        //public async Task<IActionResult> Index()
        //{

        //    return View(await _context.RoomTypes.ToListAsync());
        //}
        public async Task<IActionResult> Index(int? page, string? search)
        {
            int pageSize = 3;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var appDbContext = _context.RoomTypes.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                appDbContext = appDbContext.Where(r => r.TypeName.Contains(search));
            }
            PagedList<RoomType> lst = new PagedList<RoomType>(appDbContext, pageNumber, pageSize);
            ViewData["CurrentSearch"] = search;
            return View(lst);
        }

        // GET: Customer/RoomType/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomType = await _context.RoomTypes
                .FirstOrDefaultAsync(m => m.TypeId == id);
            if (roomType == null)
            {
                return NotFound();
            }

            return View(roomType);
        }

        // GET: Customer/RoomType/Create
        public IActionResult Create()
        {
            
            return View();
        }

        // POST: Customer/RoomType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TypeId,TypeName,TypePrice,Description")] RoomType roomType, IFormFile TypeImg)
        {
            if (ModelState.IsValid)
            {
                if (TypeImg != null)
                {
                    // Define the folder path
                    string folder = "img/";
                    // Generate a unique file name
                    string fileName = Guid.NewGuid().ToString() + "_" + TypeImg.FileName;
                    // Combine the path with the root path of the application
                    string serverFolder = Path.Combine(_webhostenvironment.WebRootPath, folder, fileName);

                    // Copy the uploaded file to the server
                    using (var fileStream = new FileStream(serverFolder, FileMode.Create))
                    {
                        await TypeImg.CopyToAsync(fileStream);
                    }

                    // Set the image path in the roomType object
                    roomType.TypeImg = "/" + folder + fileName;
                }

                // Add the room type to the context
                _context.Add(roomType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(roomType);
        }

        // GET: Customer/RoomType/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType == null)
            {
                return NotFound();
            }
            return View(roomType);
        }
        //[HttpPost]
        //public async Task<IActionResult> Edit(int id, [Bind("TypeId,TypeName,TypePrice,Description,TypeImg")] RoomType roomType)
        //{
        //    if (id != roomType.TypeId)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(roomType);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!RoomTypeExists(roomType.TypeId))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(roomType);
        //}


        // POST: Customer/RoomType/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TypeId,TypeName,TypePrice,Description,TypeImg")] RoomType roomType, IFormFile TypeImg)
        {
            if (id != roomType.TypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (TypeImg != null && TypeImg.Length > 0)
                    {
                        // Define the folder path
                        string folder = "img/";
                        // Generate a unique file name
                        string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(TypeImg.FileName);
                        // Combine the path with the root path of the application
                        string serverFolder = Path.Combine(_webhostenvironment.WebRootPath, folder, fileName);

                        // Copy the uploaded file to the server
                        using (var fileStream = new FileStream(serverFolder, FileMode.Create))
                        {
                            await TypeImg.CopyToAsync(fileStream);
                        }

                        // Update the image path in the roomType object
                        roomType.TypeImg = "/" + folder + fileName;
                    }

                    _context.Update(roomType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomTypeExists(roomType.TypeId))
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
            return View(roomType);
        }

        // GET: Customer/RoomType/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomType = await _context.RoomTypes
                .FirstOrDefaultAsync(m => m.TypeId == id);
            if (roomType == null)
            {
                return NotFound();
            }

            return View(roomType);
        }

        // POST: Customer/RoomType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType != null)
            {
                _context.RoomTypes.Remove(roomType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomTypeExists(int id)
        {
            return _context.RoomTypes.Any(e => e.TypeId == id);
        }
    }
}
