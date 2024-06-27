using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Luna.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string? FullName { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(200)]
        public string? Address { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Wallet { get; set; } = 0;
        public string? ImageUrl { get; set; }

        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public virtual ICollection<UseService> UseServices { get; set; } = new List<UseService>();
        public virtual ICollection<HotelOrder> HotelOrders { get; set; } = new List<HotelOrder>();
        public virtual ICollection<ChatMessages> SentMessages { get; set; }
        public virtual ICollection<ChatMessages> ReceivedMessages { get; set; }
        public virtual ICollection<WishList> WishLists { get; set; } = new List<WishList>();
    }
}
