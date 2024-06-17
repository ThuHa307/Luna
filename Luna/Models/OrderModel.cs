using Luna.Areas.Customer.Models;
namespace Luna.Models
{
    public class OrderModel
    {
        public HotelOrder HotelOrder { get; set; }
        public OrderDetail OrderDetail { get; set; }
        public RoomType RoomType { get; set; }
        public IEnumerable<Luna.Models.RoomType> RoomTypes { get; set; }
        public RoomImage RoomImage { get; set; }
        public RoomPromotion RoomPromotion { get; set; }
        public Promotion Promotion { get; set; }
        public Room Room { get; set; }
        public RoomOrder RoomOrder { get; set; }
        public Customer Customer { get; set; }
        public UseService UseService { get; set; }
        public Service Service { get; set; }
        public Feedback Feedback { get; set; }
        public List<RoomCart> CartItems { get; set; }
        public IEnumerable<Luna.Models.Service> Services;
        public ApplicationUser applicationUser { get; set; }
    }
}

