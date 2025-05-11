using System;
using System.Collections.Generic;
using Entities.ViewModel;

namespace Entities.ViewModel;

public partial class ItemModifierVM
{
    public int ItemId { get; set; }

    public string ModifierName { get; set; } = null!;
    public decimal? ModifierRate { get; set; }

    public int modifierId { get; set; }

    public int ModifierGroupId { get; set; }

    public string ModifierGroupName { get; set; } = null!;

    public int? MinSelection { get; set; }

    public int? MaxSelection { get; set; }

    
    public List<ModifierVM> MenuModifiers { get; set; }

    public List<MenuModifierGroupVM> MenuModifierGroupItem {get; set;}

 
}