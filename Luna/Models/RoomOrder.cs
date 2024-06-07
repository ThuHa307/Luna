using System;
using System.Collections.Generic;

namespace Luna.Models;

public partial class RoomOrder
{
    //public int TypeID {  get; set; }
    public int OrderId { get; set; }

    public int RoomId { get; set; }

    public DateOnly? CheckIn { get; set; }

    public DateOnly? CheckOut { get; set; }

    public DateTime? ConfirmCheckIn { get; set; }

    public DateTime? ConfirmCheckOut { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual HotelOrder Order { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;

    public virtual ICollection<UseService> UseServices { get; set; } = new List<UseService>();
}
