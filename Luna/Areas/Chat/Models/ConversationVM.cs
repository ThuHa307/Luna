using Luna.Models;

namespace Luna.Areas.Chat.Models
{
    public class ConversationVM
    {
        public static List<UserVM>? Users { get; set; }
        public static List<ChatMessages>? ChatMessages { get; set; }
    }
}