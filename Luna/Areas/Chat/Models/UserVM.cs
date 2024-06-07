using Luna.Models;

namespace Luna.Areas.Chat.Models
{
    public class UserVM
    {
        public ApplicationUser? User { get; set; }
        public ChatMessages? LastMessage { get; set; }
        public int? NotSeen { get; set; }
    }
}