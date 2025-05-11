using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class PaymentStatus
{
    public int PaymentStatusId { get; set; }

    public string? PaymentStatusName { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public virtual ICollection<Payment> Payments { get; } = new List<Payment>();
}
