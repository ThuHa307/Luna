using System;
using System.Collections.Generic;

namespace Luna;

public partial class UseService
{
    public int UseServiceId { get; set; }

    public DateTime? DateUseService { get; set; }

    public int? Quantity { get; set; }

    public int ServiceId { get; set; }

    public int OrderId { get; set; }

    public int RoomId { get; set; }

    public string Id { get; set; } = null!;

    public virtual AspNetUser IdNavigation { get; set; } = null!;

    public virtual RoomOrder RoomOrder { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
