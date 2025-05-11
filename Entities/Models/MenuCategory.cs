using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class MenuCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? CategoryDescription { get; set; }

    public bool? IsDeleted { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; } = new List<MenuItem>();

    public virtual ICollection<MenuModifierGroup> MenuModifierGroups { get; } = new List<MenuModifierGroup>();

    public virtual ICollection<OrderItem> OrderItems { get; } = new List<OrderItem>();
}
