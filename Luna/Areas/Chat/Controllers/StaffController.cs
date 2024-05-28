using Luna.Areas.Chat.Models;
using Luna.Data;
using Luna.Models;
using Luna.Services;
using Luna.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Luna.Areas.Chat.Controllers
{
    [Area("Chat")]
    [Authorize(Roles = Roles.Role_Consultant)]
    public class StaffController : Controller
    {
        private readonly ILogger<StaffController> _logger;
        private readonly AppDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly GlobalService _globalService;
        public StaffController(ILogger<StaffController> logger, AppDbContext dbContext, UserManager<IdentityUser> userManager, GlobalService globalService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _userManager = userManager;
            _globalService = globalService;
        }

        public IActionResult Index()
        {
            var consultantId = _globalService.GetConsultantId();
            var senderIds = _dbContext.ChatMessages
                                  .Where(m => m.SenderId != consultantId)
                                  .Select(cm => cm.SenderId)
                                  .Distinct()
                                  .ToList();
            var users = _dbContext.ApplicationUser
                              .Where(u => senderIds.Contains(u.Id))
                              .ToList();
            ConversationVM.Users = users;
            return View();
        }

        public IActionResult Conversation(string userid)
        {
            var consultantId = _globalService.GetConsultantId();
            var messages = _dbContext.ChatMessages
                            .Where(m => m.SenderId == userid || m.ReceiverId == userid)
                            .OrderBy(m => m.Timestamp)
                            .ToList();
            
            ConversationVM.ChatMessages = messages;
            ViewData["userId"] = consultantId;
            ViewData["senderId"] = userid;
            ViewData["consultantId"] = consultantId;
            return View();
        }
    }
}
