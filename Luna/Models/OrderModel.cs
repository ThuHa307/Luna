using Luna.Models;

public class OrderModel
{
    public OrderDetail OrderDetail { get; set; }
    public RoomOrder RoomOrder { get; set; }
    public IEnumerable<Luna.Models.Service> Services;
    public ApplicationUser applicationUser { get; set; }
    public HotelOrder HotelOrder { get; set; }
}