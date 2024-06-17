using Luna.Models;

public class SessionDataViewModel
{
    public int? TypeId { get; set; }
    public int? NumberOfRoom { get; set; }
    public DateOnly? CheckIn { get; set; }
    public DateOnly? CheckOut { get; set; }
    public int? OrderID { get; set; }
    public int? RoomID { get; set; }
    public string TotalPrice { get; set; }
    public List<UseService> UseServices { get; set; } // Danh sách các dịch vụ đã thêm
    public List<Service> Services { get; set; }
}
