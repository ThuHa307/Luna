using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Luna.Models;

public partial class Room
{
    public int RoomId { get; set; }
    [Required]
    public string? RoomStatus { get; set; }
    public bool? IsActive { get; set; }

    public int? TypeId { get; set; }

    public virtual ICollection<RoomOrder> RoomOrders { get; set; } = new List<RoomOrder>();

    public virtual RoomType? Type { get; set; }
}
