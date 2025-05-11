using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public DateOnly Date { get; set; }

    public int? ReviewId { get; set; }

    public string? Comment { get; set; }

    public decimal SubTotal { get; set; }

    public int NoOfPerson { get; set; }

    public decimal TotalAmount { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public int? OrderStatusId { get; set; }

    public int? PaymentModeId { get; set; }

    public string? InvoiceNo { get; set; }

    public TimeSpan? OrderDuration { get; set; }

    public DateTime? PlacedOn { get; set; }

    public string? OrderType { get; set; }

    public string? OrderInstructions { get; set; }

    public bool? IsSgstInclude { get; set; }

    public decimal? SgstAmt { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; } = new List<OrderItem>();

    public virtual OrderStatus? OrderStatus { get; set; }

    public virtual ICollection<OrderTable> OrderTables { get; } = new List<OrderTable>();

    public virtual ICollection<OrderTax> OrderTaxes { get; } = new List<OrderTax>();

    public virtual PaymentMode? PaymentMode { get; set; }

    public virtual ICollection<Payment> Payments { get; } = new List<Payment>();

    public virtual Review? Review { get; set; }

    public virtual ICollection<Review> Reviews { get; } = new List<Review>();
}
