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
        private readonly string consultantId;
        public ChatHub(AppDbContext dbContext, GlobalService globalService)
        {
            _dbContext = dbContext;
            _globalService = globalService;
            consultantId = _globalService.GetConsultantId();
        }

        public async Task SendMessageToStaff(string senderId, string message)
        {
            ChatMessages chatMessage = new ChatMessages()
            { SenderId = senderId, Message = message, ReceiverId = consultantId, Timestamp = DateTime.Now };
            _dbContext.ChatMessages.Add(chatMessage);
            _dbContext.SaveChanges();
            await Clients.User(consultantId).SendAsync("ReceiveMessage", senderId, message, chatMessage.FormattedTimestamp, consultantId);
            await Clients.Caller.SendAsync("ReceiveMessage", senderId, message, chatMessage.FormattedTimestamp, consultantId);
        }

        public async Task SendMessageToUser(string userId, string message)
        {
            ChatMessages chatMessage = new ChatMessages()
            { SenderId = consultantId, Message = message, ReceiverId = userId, Timestamp = DateTime.Now };
            _dbContext.ChatMessages.Add(chatMessage);
            _dbContext.SaveChanges();
            await Clients.User(userId).SendAsync("ReceiveMessage", consultantId, message, chatMessage.FormattedTimestamp, consultantId);
            await Clients.Caller.SendAsync("ReceiveMessage", consultantId, message, chatMessage.FormattedTimestamp, consultantId);
        }
        private static Dictionary<string, int> userTotalDictionary = new Dictionary<string, int>();
        public async Task NotSeenMess(string userId)
        {
            if (!userTotalDictionary.ContainsKey(userId))
            {
                userTotalDictionary[userId] = 0;
            }
            int count = userTotalDictionary[userId];
            count++;
            userTotalDictionary[userId] = count;
            await Clients.User(consultantId).SendAsync("CountNotSeenMess", userId, count);
        }
        public void SetCountMess(string userId)
        {
            if (userTotalDictionary.ContainsKey(userId))
            {
                userTotalDictionary[userId] = 0;
            }
        }
        public async Task SendMessNotification()
        {
            await Clients.User(consultantId).SendAsync("GetMessNotification");
        }
    }
}
