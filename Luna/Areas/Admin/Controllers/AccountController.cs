using Luna.Data;
using Luna.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Luna.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Luna.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Luna.Areas.Admin.Models;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Formats.Asn1.AsnWriter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.IdentityModel.Tokens;


namespace Luna.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        // Use this constructor for dependency injection
        [ActivatorUtilitiesConstructor]
        public AccountController(AppDbContext db, RoleManager<IdentityRole> roleManager, 
            IUserStore<IdentityUser> userStore, UserManager<IdentityUser> userManager,
            IServiceScopeFactory serviceScopeFactory)
        {
            _db = db;
            _roleManager = roleManager;
            _userStore = userStore ;
            //_emailStore = GetEmailStore();
            _userManager = userManager;
            _serviceScopeFactory = serviceScopeFactory;
        }




        public async Task<IActionResult> Index()
        {
            // Lấy danh sách người dùng từ database
            List<ApplicationUser> listaccount = _db.ApplicationUser.ToList();
            List<ApplicationUser> receptionists = new List<ApplicationUser>();

            foreach (var user in listaccount)
            {
                //lấy role
                var roles = await _userManager.GetRolesAsync(user);
                // nếu là Receptionist thì add vào list
                if (roles.Contains("Receptionist"))
                {
                    receptionists.Add(user);
                }
            }

            return View(receptionists);
        }



        public IActionResult Addccount()
        {

            return View();
        }
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return View("Index", await _db.ApplicationUser.ToListAsync());
            }

            // Fetch all users from the database asynchronously
            List<ApplicationUser> listaccount = await _db.ApplicationUser.ToListAsync();

            List<ApplicationUser> receptionists = new List<ApplicationUser>();
            foreach (var user in listaccount)
            {
                // Get roles for the user asynchronously
                var roles = await _userManager.GetRolesAsync(user);

                // If the user has the 'Receptionist' role, add to the list
                if (roles.Contains("Receptionist"))
                {
                    receptionists.Add(user);
                }
            }

            // Perform the search within the list of receptionists
            var staffs = receptionists
                .Where(s => s.UserName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            s.Email.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            s.PhoneNumber.Contains(query))
                .ToList();

            return View("Index", staffs);
        }


        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }
        // POST: Account/Create
        [HttpPost]
        public async Task<IActionResult> Create(StaffInfor model)
        {
            Console.WriteLine($"code da qua day  modestate.isvalid = {ModelState.IsValid}");
            if (ModelState.IsValid)
            {
                //var user = CreateUser();
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, FullName=model.FullName,
                                                 DateOfBirth = model.DateOfBirth,
                                                 PhoneNumber = model.PhoneNumber,
                                                 Address=model.Address};
                

               
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {                   
                    await _userManager.AddToRoleAsync(user, Roles.Role_Receptionist);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var result1 = await _userManager.ConfirmEmailAsync(user, code);
                }

            }


            Console.WriteLine("DONE");
            // If we got this far, something failed; redisplay form
            return RedirectToAction("Index");
        }
        private ApplicationUser CreateUser()
        {
            try
            {
                Console.WriteLine("create user");
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                Console.WriteLine("loi create user");
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _db.ApplicationUser.FindAsync(id);
            return View(user);
        }
        public async Task<IActionResult> Edit(string Id)
        {

            var staff = await _db.ApplicationUser.FindAsync(Id);
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,Email,PhoneNumber,Address,DateOfBirth,FullName")] ApplicationUser updatedUser)
        {
            

            //if (!ModelState.IsValid)
            //{
            //    Console.WriteLine($"1111111111 Loi !ModelState.IsValid = {ModelState.IsValid}");
            //    return View(updatedUser);
            //}

            try
            {
                var existingUser = await _db.ApplicationUser.FindAsync(updatedUser.Id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                
                existingUser.Email = updatedUser.Email;
                existingUser.PhoneNumber = updatedUser.PhoneNumber;
                existingUser.Address = updatedUser.Address;
                existingUser.DateOfBirth = updatedUser.DateOfBirth;
                existingUser.FullName = updatedUser.FullName;

                _db.Update(existingUser);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Xử lý ngoại lệ DbUpdateConcurrencyException
                Console.WriteLine($"AAAAA Loi {ex.Message}");
                // Hiển thị lại form với thông báo lỗi nếu có lỗi xảy ra
                return View(updatedUser);
            }
        }



    }
}
