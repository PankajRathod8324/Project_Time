using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Unit
{
    public int UnitId { get; set; }

    public string UnitName { get; set; } = null!;

    public virtual ICollection<MenuItem> MenuItems { get; } = new List<MenuItem>();

    public virtual ICollection<MenuModifier> MenuModifiers { get; } = new List<MenuModifier>();
}
