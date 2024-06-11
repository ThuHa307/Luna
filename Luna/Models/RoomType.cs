﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Luna.Models;

public partial class RoomType
{
    public int TypeId { get; set; }
    public string? TypeName { get; set; }
    public decimal? TypePrice { get; set; }
    public string? Description { get; set; }
    public string? TypeImg { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();

    public virtual ICollection<RoomPromotion> RoomPromotions { get; set; } = new List<RoomPromotion>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
