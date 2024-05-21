using System;
using System.Collections.Generic;

namespace Luna;

public partial class HotelOrder
{
    public int OrderId { get; set; }

    public DateOnly? OrderDate { get; set; }

    public string? OrderStatus { get; set; }

    public double? Deposits { get; set; }

    public string Id { get; set; } = null!;

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual AspNetUser IdNavigation { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<RoomOrder> RoomOrders { get; set; } = new List<RoomOrder>();
}
