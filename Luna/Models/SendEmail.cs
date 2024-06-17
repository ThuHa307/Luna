namespace Luna.Models
{
    public class SendEmail
    {
        public int? OrderId { get; set; }
        public int? TotalRoom { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public double? Deposits { get; set; }
        public DateOnly? Checkin { get; set; }
        public DateOnly? Checkout { get; set; }
    }
}
