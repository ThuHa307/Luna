using System;
using System.Collections.Generic;

namespace Luna;

public partial class Feedback
{
    public string? Message { get; set; }

    public int OrderId { get; set; }

    public string Id { get; set; } = null!;

    public virtual AspNetUser IdNavigation { get; set; } = null!;

    public virtual HotelOrder Order { get; set; } = null!;
}
