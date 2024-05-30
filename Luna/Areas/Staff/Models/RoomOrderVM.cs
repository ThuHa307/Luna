using System.Security.AccessControl;
using CustomerModel = Luna.Models.Customer;

namespace Luna.Areas.Staff.Models
{
    public class RoomOrderVM
    {
        public int OrderId { get; set; }
        public int RoomId { get; set; }

        public List<CustomerModel>? Customers { get; set; }
    }
}
