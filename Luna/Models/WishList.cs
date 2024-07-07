using System;
using System.Collections.Generic;

namespace Luna.Models;

public partial class WishList
{
    public int TypeId { get; set; }

    public string UserId { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual RoomType RoomType { get; set; } = null!;
}
