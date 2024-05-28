using Luna.Models;

namespace Luna.Areas.Chat.Models
{
    public class ConversationVM
    {
        public static List<ApplicationUser> Users { get; set; }
        public static List<ChatMessages> ChatMessages { get; set; }
    }
}
