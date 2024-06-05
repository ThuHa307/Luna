using System;
using System.Collections.Generic;

namespace Luna.Models;

public partial class ChatMessages
{
    public int Id { get; set; }

    public string? SenderId { get; set; }

    public string? ReceiverId { get; set; }

    public string? Message { get; set; }
    public bool? IsSeen { get; set; }
    public DateTime Timestamp { get; set; }
    public virtual ApplicationUser Sender { get; set; } = null!;
    public virtual ApplicationUser Receiver { get; set; } = null!;

    public string FormattedTimestamp => Timestamp.ToString("dd MMM yyyy, HH:mm");
}
