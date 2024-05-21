using System;
using System.Collections.Generic;

namespace Luna;

public partial class RoomImage
{
    public int Id { get; set; }

    public string? Link { get; set; }

    public int? TypeId { get; set; }

    public virtual RoomType? Type { get; set; }
}
