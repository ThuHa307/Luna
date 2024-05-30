using System;
using System.Collections.Generic;

namespace Luna.Models;

public partial class Customer
{
    public int OrderId { get; set; }

    public int RoomId { get; set; }

    public int CustomerId { get; set; }

    public string? CusName { get; set; }

    public string? Cccd { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Genre { get; set; }

    public string? Address { get; set; }

    public virtual RoomOrder RoomOrder { get; set; } = null!;
}
