using System;
using System.Collections.Generic;

namespace Luna.Models;

public partial class RoomPromotion
{
    public int PromotionId { get; set; }
    public int TypeId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public virtual Promotion Promotion { get; set; } = null!;
    public virtual RoomType Type { get; set; } = null!;
}
