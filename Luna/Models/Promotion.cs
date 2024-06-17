using System;
using System.Collections.Generic;

namespace Luna.Models;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string? Title { get; set; }

    public double? Discount { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public string? PromotionImg {  get; set; }

    public virtual ICollection<RoomPromotion> RoomPromotions { get; set; } = new List<RoomPromotion>();
}
