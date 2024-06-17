using Luna.Models;

namespace Luna.Areas.Customer.Models
{
    public class ViewDetailModel
    {
        public RoomType RoomType { get; set; }
        public List<RoomSearchViewModel> AvailableRooms { get; set; }
    }
}
