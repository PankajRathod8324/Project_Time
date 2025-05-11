using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class MenuItemUnit
{
    public int UnitId { get; set; }

    public string UnitName { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }
}
