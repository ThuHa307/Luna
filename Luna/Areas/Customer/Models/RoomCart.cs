using Luna.Models;

namespace Luna.Areas.Customer.Models
{
    public class RoomCart
    {
        public int TypeId { get; set; }

        public string? TypeName { get; set; }

        public decimal? TypePrice { get; set; }

        public string? Description { get; set; }
        public string? TypeImg { get; set; }
        public int Quantity { get; set; }
        public DateOnly? CheckIn { get; set; }
        public DateOnly? CheckOut { get; set; }
        public decimal? total
        {
            get { return Quantity * TypePrice; }
        }
        public RoomCart()
        {

        }
        public RoomCart(RoomType room, int Quantity, DateOnly? checkIn, DateOnly? checkOut,decimal typePrice)
        {
            TypeId = room.TypeId;
            TypeName = room.TypeName;
            TypePrice = typePrice;
            Description = room.Description;
            TypeImg = room.TypeImg;
            this.Quantity = Quantity;
            CheckIn = checkIn;
            CheckOut = checkOut;
        }
    }
}
