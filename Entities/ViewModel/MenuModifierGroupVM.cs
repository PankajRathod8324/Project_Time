using System.ComponentModel.DataAnnotations;
using Entities.Models;
using X.PagedList;

namespace Entities.ViewModel
{
    public class MenuModifierGroupVM
    {
        public IEnumerable<MenuModifierGroup>? menuModifierGroups { get; set; }

        // public MenuCategory EditCategory { get; set; }

        public List<ItemModifierVM>? itemModifierGroups { get; set; }

         public List<int> ModifierIds { get; set; }

           public List<ModifierVM> Modifiers { get; set; }
        public int ModifierGroupId { get; set; }

         public int ItemId { get; set; }

        public List<int> ModifierGroupIds { get; set; } = new List<int>(); 

        public string ModifierGroupName { get; set; } = null!;

        public string ModifierGroupDecription { get; set; } = null!;

        public bool? IsDeleted { get; set; }



        public List<MenuModifierGroupVM>? menuModifiers { get; set; }

        public IPagedList<MenuModifierGroupVM>? Modifier { get; set; }

        public int ModifierId { get; set; }

        public int MinSelection {get; set;}

        public int MaxSelection {get; set;}
        
        public string ModifierName { get; set; } = null!;

        public string ItemInstruction {get; set;} = null!;

        public decimal? ModifierRate { get; set; }

        public int? CategoryId { get; set; }

        public int? UnitId { get; set; }

        public int Quantity { get; set; }

        public string ModifierDecription { get; set; } = null!;



        public string UnitName { get; set; }

      

    }
}