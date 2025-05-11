using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class CombineModifier
{
    public int CombineModifierId { get; set; }

    public int? ModifierGroupId { get; set; }

    public int? ModifierId { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual MenuModifier? Modifier { get; set; }

    public virtual MenuModifierGroup? ModifierGroup { get; set; }
}
