using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class MenuModifierGroup
{
    public int ModifierGroupId { get; set; }

    public string ModifierGroupName { get; set; } = null!;

    public string? ModifierGroupDecription { get; set; }

    public bool? IsDeleted { get; set; }

    public int? CategoryId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public virtual MenuCategory? Category { get; set; }

    public virtual ICollection<CombineModifier> CombineModifiers { get; } = new List<CombineModifier>();

    public virtual ICollection<ItemModifierGroup> ItemModifierGroups { get; } = new List<ItemModifierGroup>();

    public virtual ICollection<MenuItem> MenuItems { get; } = new List<MenuItem>();

    public virtual ICollection<MenuModifier> MenuModifiers { get; } = new List<MenuModifier>();
}
