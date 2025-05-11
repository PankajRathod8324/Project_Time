using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class PaymentMode
{
    public int PaymentModeId { get; set; }

    public string? PaymentModeName { get; set; }

    public virtual ICollection<Order> Orders { get; } = new List<Order>();
}
