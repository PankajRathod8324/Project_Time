using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class MenuItem
{
    public int ItemId { get; set; }

    public int? CategoryId { get; set; }

    public string ItemName { get; set; } = null!;

    public decimal Rate { get; set; }

    public int Quantity { get; set; }

    public int? UnitId { get; set; }

    public bool? IsAvailable { get; set; }

    public bool TaxDefault { get; set; }

    public decimal TaxPercentage { get; set; }

    public string? ShortCode { get; set; }

    public string? Description { get; set; }

    public string? CategoryPhoto { get; set; }

    public bool? IsFavourite { get; set; }

    public int? ModifierGroupId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public int? ItemtypeId { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual MenuCategory? Category { get; set; }

    public virtual ICollection<Favourite> Favourites { get; } = new List<Favourite>();

    public virtual ICollection<ItemModifierGroup> ItemModifierGroups { get; } = new List<ItemModifierGroup>();

    public virtual Itemtype? Itemtype { get; set; }

    public virtual MenuModifierGroup? ModifierGroup { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; } = new List<OrderItem>();

    public virtual Unit? Unit { get; set; }
}
