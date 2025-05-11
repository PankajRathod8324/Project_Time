using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class MenuModifier
{
    public int ModifierId { get; set; }

    public string ModifierName { get; set; } = null!;

    public int? ModifierGroupId { get; set; }

    public decimal? ModifierRate { get; set; }

    public bool? IsDeleted { get; set; }

    public int? UnitId { get; set; }

    public int Quantity { get; set; }

    public string ModifierDecription { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public virtual ICollection<CombineModifier> CombineModifiers { get; } = new List<CombineModifier>();

    public virtual MenuModifierGroup? ModifierGroup { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; } = new List<OrderItem>();

    public virtual ICollection<OrderModifier> OrderModifiers { get; } = new List<OrderModifier>();

    public virtual Unit? Unit { get; set; }
}
