using Luna.Models;

namespace Luna.Areas.Chat.Models
{
    public class ChatMessagesVM
    {
        public ChatMessages? ChatMessages { get; set; }
        public string? RelativeTime { get; set; }
        public int? NotSeen { get; set; }
    }
}
