using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class ItemModifierGroup
{
    public int ItemId { get; set; }

    public int ModifierGroupId { get; set; }

    public int? MinSelection { get; set; }

    public int? MaxSelection { get; set; }

    public virtual MenuItem Item { get; set; } = null!;

    public virtual MenuModifierGroup ModifierGroup { get; set; } = null!;
}
