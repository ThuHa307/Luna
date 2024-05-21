using System;
using System.Collections.Generic;

namespace Luna.Models;

public partial class OrderDetail
{
    public int? NumberOfRoom { get; set; }

    public int TypeId { get; set; }

    public int OrderId { get; set; }

    public virtual HotelOrder Order { get; set; } = null!;

    public virtual RoomType Type { get; set; } = null!;
}
