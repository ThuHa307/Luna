using System;
using System.Collections.Generic;

namespace Luna.Models;

public partial class UseService
{
    public int UseServiceId { get; set; }

    public DateTime? DateUseService { get; set; }

    public int? Quantity { get; set; }

    public int ServiceId { get; set; }

    public int OrderId { get; set; }

    public int RoomId { get; set; }

    public string Id { get; set; } = null!;
    public virtual RoomOrder RoomOrder { get; set; }
    public virtual ApplicationUser User { get; set; }
    public virtual Service Service { get; set; } 
    
}
