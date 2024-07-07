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
using X.PagedList;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json;



namespace Luna.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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




        public async Task<IActionResult> Index(int? page)
        {

            //1 page/10 nguoi
            int pageSize = 10;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
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
                    if(user.EmailConfirmed)
                    {
                        receptionists.Add(user);
                    }
                    
                }
            }

            PagedList<ApplicationUser> lst = new PagedList<ApplicationUser>(receptionists, pageNumber, pageSize);

            //return View(receptionists);
            return View(lst);
        }



        public async Task<IActionResult> Search(string query)
        {
            
            if (string.IsNullOrEmpty(query))
            {
                List<ApplicationUser> listaccount1 = _db.ApplicationUser.ToList();
                List<ApplicationUser> receptionists1 = new List<ApplicationUser>();

                foreach (var user in listaccount1)
                {
                    //lấy role
                    var roles = await _userManager.GetRolesAsync(user);
                    // nếu là Receptionist thì add vào list
                    if (roles.Contains("Receptionist"))
                    {
                        if (user.EmailConfirmed)
                        {
                            receptionists1.Add(user);
                        }

                    }
                }

                return View("Index", receptionists1);
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
                    if (user.EmailConfirmed)
                    {
                        receptionists.Add(user);
                    }
                    
                }
            }

            // Perform the search within the list of receptionists
            var staffs = receptionists
                .Where(s => s.UserName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            s.Email.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            s.PhoneNumber.Contains(query)||
                            s.Address.Contains(query)||
                            s.FullName.Contains(query))
                .ToList();

            return View("Index", staffs);
        }


        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Account/Create
        //[HttpPost]
        //public async Task<IActionResult> Create(StaffInfor model, IFormFile? ImageUrl)
        //{
        //    Console.WriteLine($"code da qua day  modestate.isvalid = {ModelState.IsValid}");
        //    if (ModelState.IsValid)
        //    {
        //        //var user = CreateUser();
        //        string Image = "thuha.jpg";
        //        if (ImageUrl != null)
        //        {
        //            // Generate a unique file name using GUID
        //            var fileExtension = Path.GetExtension(ImageUrl.FileName);
        //            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        //            Image = uniqueFileName;
        //            var filePath = Path.Combine("wwwroot/images", uniqueFileName);
        //            using (var fileStream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await ImageUrl.CopyToAsync(fileStream);
        //            }
        //        }
        //        var user = new ApplicationUser
        //        {
        //            UserName = model.UserName,
        //            Email = model.Email,
        //            FullName = model.FullName,
        //            DateOfBirth = model.DateOfBirth,
        //            PhoneNumber = model.PhoneNumber,
        //            Address = model.Address,
        //            ImageUrl = Image
        //        };

        //        // Print the properties of the user object to the console
        //        Console.WriteLine($"UserName: {user.UserName}");
        //        Console.WriteLine($"Email: {user.Email}");
        //        Console.WriteLine($"FullName: {user.FullName}");
        //        Console.WriteLine($"DateOfBirth: {user.DateOfBirth?.ToString("yyyy-MM-dd") ?? "N/A"}");
        //        Console.WriteLine($"PhoneNumber: {user.PhoneNumber}");
        //        Console.WriteLine($"Address: {user.Address}");
        //        Console.WriteLine($"ImageUrl: {user.ImageUrl}");



        //        var result = await _userManager.CreateAsync(user, model.Password);
        //        Console.WriteLine($"check  result.Succeeded = {result.Succeeded}");


        //        // Populate errorList
        //        if (!result.Succeeded)
        //        {
        //            foreach (var error in result.Errors)
        //            {
        //                Console.WriteLine($"Error: {error.Code} - {error.Description}");
        //            }

        //        }
        //        if (result.Succeeded)
        //        {                   
        //            await _userManager.AddToRoleAsync(user, Roles.Role_Receptionist);
        //            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //            var result1 = await _userManager.ConfirmEmailAsync(user, code);
        //        }

        //    }

        //    Console.WriteLine("DONE");
        //    // If we got this far, something failed; redisplay form
        //    return RedirectToAction("Index");
        //}
        [HttpPost]
        public async Task<IActionResult> Create(StaffInfor model, IFormFile? ImageUrl)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUserByUsername = await _userManager.FindByNameAsync(model.UserName);
                if (existingUserByUsername != null)
                {
                    ModelState.AddModelError("UserName", "Username already exists.");
                    return View(model);
                }

                // Check if email already exists
                var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }

                string Image = "thuha.jpg";
                if (ImageUrl != null)
                {
                    var fileExtension = Path.GetExtension(ImageUrl.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                    Image = uniqueFileName;
                    var filePath = Path.Combine("wwwroot/images", uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageUrl.CopyToAsync(fileStream);
                    }
                }

                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    DateOfBirth = model.DateOfBirth,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    ImageUrl = Image
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Roles.Role_Receptionist);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var result1 = await _userManager.ConfirmEmailAsync(user, code);

                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed; redisplay form
            return View(model);
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
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,Email,PhoneNumber,Address,DateOfBirth,FullName")] ApplicationUser updatedUser, IFormFile? ImageUrl)
        {
            

           
            try
            {
                var existingUser = await _db.ApplicationUser.FindAsync(updatedUser.Id);
                string Image= existingUser.ImageUrl ;
                if (ImageUrl != null)
                {
                    // Generate a unique file name using GUID
                    var fileExtension = Path.GetExtension(ImageUrl.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                    Image = uniqueFileName;
                    var filePath = Path.Combine("wwwroot/images", uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageUrl.CopyToAsync(fileStream);
                    }
                }
                if (existingUser == null)
                {
                    return NotFound();
                }

                
                existingUser.Email = updatedUser.Email;
                existingUser.PhoneNumber = updatedUser.PhoneNumber;
                existingUser.Address = updatedUser.Address;
                existingUser.DateOfBirth = updatedUser.DateOfBirth;
                existingUser.FullName = updatedUser.FullName;
                existingUser.ImageUrl = Image;

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
        [HttpGet]
        public IActionResult Delete( string id)
        {
            var staff = _db.ApplicationUser.Find(id);
            return View(staff);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _db.Users.FindAsync(id);
            user.EmailConfirmed = false;
            _db.Update(user);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




    }
}
