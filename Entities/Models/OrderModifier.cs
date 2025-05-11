using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class OrderModifier
{
    public int OrderModifierId { get; set; }

    public int? ModifierId { get; set; }

    public DateOnly Date { get; set; }

    public int Quantity { get; set; }

    public decimal Rate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public int? OrderItemId { get; set; }

    public int? ItemId { get; set; }

    public string? Uid { get; set; }

    public virtual MenuModifier? Modifier { get; set; }

    public virtual OrderItem? OrderItem { get; set; }
}
