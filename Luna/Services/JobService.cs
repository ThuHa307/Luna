using Luna.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Luna.Services
{
    public class JobService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEmailSender _emailSender;

        public JobService(IEmailSender emailSender, AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _emailSender = emailSender;
        }

        public void CheckDiscountsAndSendEmails()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var checkDate = today.AddDays(2);

            var roomPromotions = _dbContext.RoomPromotions
                                    .Where(d => d.StartDate == checkDate)
                                    .ToList();
            foreach (var discount in roomPromotions)
            {
                var promotion = _dbContext.Promotions.Where(p => p.PromotionId == discount.PromotionId).FirstOrDefault();
                var roomTypeId = discount.TypeId;
                var roomTypeName = _dbContext.RoomTypes
                                    .Where(r => r.TypeId == roomTypeId)
                                    .Select(w => w.TypeName)
                                    .FirstOrDefault();
                var users = _dbContext.WishLists
                                    .Where(w => w.TypeId == roomTypeId)
                                    .Select(w => w.User)
                                    .ToList();

                foreach (var user in users)
                {
                    _emailSender.SendEmailAsync(user.Email, "Promotion Announcement", $"<div class='container' style='background-color: #ffffff; margin: 50px auto; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); max-width: 600px;'>\r\n    <div style='text-align: center; padding-bottom: 20px; border-bottom: 1px solid #dddddd;'>\r\n        <h2 style='margin: 0; font-size: 24px;'>Exclusive Discount on Your Favorite Room Type!</h2>\r\n    </div>\r\n    <div style='padding: 20px;'>\r\n        <h3 style='color: #333333;'>Dear {user.FullName},</h3>\r\n        <p style='color: #666666; line-height: 1.5;'>We hope this email finds you well.</p>\r\n        <p style='color: #666666; line-height: 1.5;'>We are excited to inform you about an exclusive discount on one of your favorite room types at Luna! As a valued member of our community, we wanted to give you a heads-up about this special offer before anyone else.</p>\r\n        <p style='color: #666666; line-height: 1.5;'><strong>Details of the Discount:</strong></p>\r\n        <ul style='color: #666666; line-height: 1.5;'>\r\n            <li><strong>Room Type:</strong> {roomTypeName}</li>\r\n            <li><strong>Discount:</strong> {promotion.Discount}%</li>\r\n            <li><strong>Valid From:</strong> {discount.StartDate}</li>\r\n            <li><strong>Valid Until:</strong> {discount.EndDate}</li>\r\n        </ul>\r\n        <p style='color: #666666; line-height: 1.5;'>We recommend booking early to ensure availability, as this exclusive offer is for a limited time only. Don’t miss out on the chance to enjoy a luxurious stay at a reduced price!</p>\r\n        <p style='color: #666666; line-height: 1.5;'>If you have any questions or need assistance with your booking, please don't hesitate to contact our customer service team at <i>hoteldelluna@gmail.com</i></p>\r\n        <p style='color: #666666; line-height: 1.5;'>Thank you for choosing Luna. We look forward to welcoming you soon!</p>\r\n        <a href='https://localhost:7185/' style='display: inline-block; margin-top: 20px; padding: 10px 20px; background-color: #007bff; color: #ffffff; text-decoration: none; border-radius: 5px;'>Book Now</a>\r\n    </div>\r\n    <div style='text-align: center; padding-top: 20px; border-top: 1px solid #dddddd; color: #888888;'>\r\n        <p>&copy; 2024 Luna. All rights reserved.</p>\r\n    </div>\r\n</div>");                    
                }
            }
        }
    }

}
