using Luna.Areas.Chat.Models;
using Luna.Data;
using Luna.Models;
using Luna.Services;
using Luna.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            var lastMessages = _dbContext.ChatMessages
                      .Where(m => senderIds.Contains(m.SenderId))
                      .GroupBy(m => m.SenderId)
                      .Select(g => g.OrderByDescending(m => m.Timestamp).FirstOrDefault())
                      .ToList();
            var userVMs = (from user in users
                           join message in lastMessages on user.Id equals message.SenderId into userMessages
                           from userMessage in userMessages.DefaultIfEmpty()
                           select new UserVM
                           {
                               User = user,
                               LastMessage = userMessage
                           }).ToList();
            var unseenMessageCounts = _dbContext.ChatMessages
                                    .Where(m => (bool)!m.IsSeen && m.SenderId != consultantId)
                                    .GroupBy(m => m.SenderId)
                                    .Select(g => new
                                    {
                                        SenderId = g.Key,
                                        UnseenMessageCount = g.Count()
                                    })
                                    .ToList();
            var finalUserVMs = userVMs
                   .GroupJoin(unseenMessageCounts,
                              userVM => userVM.User.Id,
                              unseen => unseen.SenderId,
                              (userVM, unseen) => new { userVM, unseen = unseen.FirstOrDefault() })
                   .Select(result => {
                       result.userVM.NotSeen = result.unseen?.UnseenMessageCount ?? 0;
                       return result.userVM;
                   })
                   .ToList();
            ConversationVM.Users = finalUserVMs;
            return View();
        }

        public IActionResult Conversation(string userid)
        {
            var consultantId = _globalService.GetConsultantId();
            var messages = _dbContext.ChatMessages
                            .Where(m => m.SenderId == userid || m.ReceiverId == userid)
                            .OrderBy(m => m.Timestamp)
                            .ToList();
            _dbContext.Database.ExecuteSqlRaw("UPDATE ChatMessages SET IsSeen = 1 WHERE SenderId = {0} AND IsSeen = 0", userid);
            ConversationVM.ChatMessages = messages;
            var userToUpdate = ConversationVM.Users.FirstOrDefault(u => u.User.Id == userid);
            if (userToUpdate != null)
            {
                userToUpdate.NotSeen = 0;
            }
            ViewData["userId"] = consultantId;
            ViewData["senderId"] = userid;
            ViewData["consultantId"] = consultantId;
            return View();
        }

    }
}
