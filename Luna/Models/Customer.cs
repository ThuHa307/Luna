﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Luna.Models;

public partial class Customer
{
    public int OrderId { get; set; }

    public int RoomId { get; set; }

    public int CustomerId { get; set; }

    [Required]
    public string? CusName { get; set; }
    [Required]
    public string? Cccd { get; set; }
    [Required]
    public DateOnly? DateOfBirth { get; set; }
    [Required]
    public string? Genre { get; set; }
    [Required]
    public string? Address { get; set; }

    public virtual RoomOrder RoomOrder { get; set; } = null!;
}
