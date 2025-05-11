using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int? ItemId { get; set; }

    public DateOnly Date { get; set; }

    public int Quantity { get; set; }

    public int Readyitemquantity { get; set; }

    public decimal Rate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public int? OrderId { get; set; }

    public int? ModifierId { get; set; }

    public int? CategoryId { get; set; }

    public string? ItemInstructions { get; set; }

    public string? Status { get; set; }

    public string? Uid { get; set; }

    public virtual MenuCategory? Category { get; set; }

    public virtual MenuItem? Item { get; set; }

    public virtual MenuModifier? Modifier { get; set; }

    public virtual Order? Order { get; set; }

    public virtual ICollection<OrderModifier> OrderModifiers { get; } = new List<OrderModifier>();
}
