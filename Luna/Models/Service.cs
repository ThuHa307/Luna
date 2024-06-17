using System;
using System.Collections.Generic;

namespace Luna.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public decimal ServicePrice { get; set; }

    public bool? IsActive { get; set; }
    public string? Description { get; set; }
    public string? ServiceImg { get; set; }

    public virtual ICollection<UseService> UseServices { get; set; } = new List<UseService>();
}
