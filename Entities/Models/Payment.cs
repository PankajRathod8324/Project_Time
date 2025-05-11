using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? OrderId { get; set; }

    public int? PaymentStatusId { get; set; }

    public decimal? Amount { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public string? PaymentMode { get; set; }

    public virtual Order? Order { get; set; }

    public virtual PaymentStatus? PaymentStatus { get; set; }
}
