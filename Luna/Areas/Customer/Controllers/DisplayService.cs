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
		public async Task<IActionResult> Index(int? page)
		{
			int pageSize = 3;
			int pageNumber = page == null || page < 0 ? 1 : page.Value;
			var appDbContext = _context.Services.AsQueryable();
			PagedList<Service> lst = new PagedList<Service>(appDbContext, pageNumber, pageSize);
			ViewBag.TotalServices = await _context.Services.CountAsync();
			ViewBag.CurrentPage = pageNumber;
			return View(lst);
		}
	}
}
