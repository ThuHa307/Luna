using Luna.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Luna.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class DisplayService : Controller
	{
		private readonly AppDbContext _context;
		private const int PageSize = 4;

		public DisplayService(AppDbContext context)
		{
			_context = context;
		}

		// GET: Services
		public async Task<IActionResult> Index(int pageNumber = 1)
		{
			var services = await _context.Services
				.OrderBy(s => s.ServiceId)
				.Skip((pageNumber - 1) * PageSize)
				.Take(PageSize)
				.ToListAsync();

			ViewBag.TotalServices = await _context.Services.CountAsync();
			ViewBag.CurrentPage = pageNumber;
			ViewBag.PageSize = PageSize;
			return View(services);
		}
	}
}
