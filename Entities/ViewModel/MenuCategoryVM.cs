using System.ComponentModel.DataAnnotations;
using Entities.Models;
using X.PagedList;

namespace Entities.ViewModel
{
    public class MenuCategoryVM
    {
        public IEnumerable<MenuCategory>? menuCategories { get; set; }

        // public MenuCategory EditCategory { get; set; }

        public IPagedList<MenuItem>? menuItems { get; set; }

        public MenuItem OrderItem { get; set; }

        public List<ItemModifierVM> ModifierGroupIds { get; set; }


        public List<OrderTaxVM> OrderTax { get; set; } = new List<OrderTaxVM>();

        public List<int> ModifierGroupIdForAdd { get; set; }

        public List<MenuModifierGroupVM> MenuItemModifier { get; set; }

        public List<TaxVM> OrderTaxes { get; set; }

        public List<TaxVM> IsDefaultTaxes { get; set; }

        public List<MenuItem> Items { get; set; }

        public string UId { get; set; }
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Item Name is required")]
        // [UniqueItemName(ErrorMessage = "Item Name already exists")]
        // Uncomment the above line after defining the UniqueItemName attribute or adding the correct namespace.
        public string ItemName { get; set; } = null!;

        public string ItemInstruction { get; set; } = null!;

        public string OrderComment { get; set; } = null!;

        public decimal Rate { get; set; }

        public int Quantity { get; set; }

        public int? UnitId { get; set; }

        public decimal SubTotal { get; set; }

        public decimal FinalTotal { get; set; }

        public decimal TotalAmount { get; set; }
        public int OrderId { get; set; }

        public int? CustomerId { get; set; }

        public bool? IsSgstInclude { get; set; }

        public bool? IsSgstIncluded { get; set; }

        public string OrderType { get; set; } = null!;

        public decimal? SgstAmt { get; set; }

        public bool IsAvailable { get; set; }

        public bool TaxDefault { get; set; }

        public decimal TaxPercentage { get; set; }

        public string? ShortCode { get; set; }

        public string? Description { get; set; }

        public string? CategoryPhoto { get; set; }

        public string ItemPhoto { get; set; }

        public bool? IsFavourite { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? CategoryDescription { get; set; }

        public List<CustomerTableVM> customerTables { get; set; }

        public List<Taxis> Taxes { get; set; }


        public bool? IsDeleted { get; set; }

        public int MinSelection { get; set; }
        public int MaxSelection { get; set; }

        public int ItemId { get; set; }

        public int? ItemtypeId { get; set; }

        public string Itemtype { get; set; }

        public int? ModifierGroupId { get; set; }



        public string? PaymentModeName { get; set; }
        public string UnitName { get; set; }


    }
}