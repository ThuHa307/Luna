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
    }
}
