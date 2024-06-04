using Luna.Models;

namespace Luna.Areas.Customer.Models
{
    public class BookingCart
    {
        public List<RoomCart>? items {  get; set; }
        public decimal? totalPrice { get; set; }

       

    }
}
