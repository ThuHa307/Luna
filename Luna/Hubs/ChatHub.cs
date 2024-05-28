using Luna.Data;
using Luna.Models;
using Luna.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Luna.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _dbContext;
        private readonly GlobalService _globalService;

        public ChatHub(AppDbContext dbContext, GlobalService globalService)
        {
            _dbContext = dbContext;
            _globalService = globalService;
        }

        public async Task SendMessageToStaff(string senderId, string message)
        {
            var consultantId = _globalService.GetConsultantId();
            ChatMessages chatMessage = new ChatMessages() 
            { SenderId = senderId, Message = message, ReceiverId = consultantId, Timestamp = DateTime.Now };
            _dbContext.ChatMessages.Add(chatMessage);
            _dbContext.SaveChanges();
            await Clients.User(consultantId).SendAsync("ReceiveMessage", senderId, message, chatMessage.FormattedTimestamp, consultantId);
            await Clients.Caller.SendAsync("ReceiveMessage", senderId, message, chatMessage.FormattedTimestamp, consultantId);
        }

        public async Task SendMessageToUser(string userId, string message)
        {
            var consultantId = _globalService.GetConsultantId();
            ChatMessages chatMessage = new ChatMessages()
            { SenderId = consultantId, Message = message, ReceiverId = userId, Timestamp = DateTime.Now };
            _dbContext.ChatMessages.Add(chatMessage);
            _dbContext.SaveChanges();
            await Clients.User(userId).SendAsync("ReceiveMessage", consultantId, message, chatMessage.FormattedTimestamp, consultantId);
            await Clients.Caller.SendAsync("ReceiveMessage", consultantId, message, chatMessage.FormattedTimestamp, consultantId);
        }
    }
}
