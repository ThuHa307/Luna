using System;
using System.ComponentModel.DataAnnotations;

namespace Luna.Areas.Admin.Models
{
    public class StaffInfor
    {
        [Required(ErrorMessage = "Full Name is required")]
        [MaxLength(100, ErrorMessage = "Full Name cannot be longer than 100 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        public DateTime DateOfBirth { get; set; }

        public string? ImageUrl { get; set; }
    }
}
