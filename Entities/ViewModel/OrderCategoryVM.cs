using System.ComponentModel.DataAnnotations;
using Entities.Models;
using X.PagedList;

namespace Entities.ViewModel
{
    public class OrderCategoryVM
    {
        public List<OrderVM> Orders { get; set; }
        public List<Order> OrderList { get; set; }


        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Item Name is required")]
        // [UniqueItemName(ErrorMessage = "Item Name already exists")]
        // Uncomment the above line after defining the UniqueItemName attribute or adding the correct namespace.
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

        public string ItemPhoto {get; set;}

        public bool? IsFavourite { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? CategoryDescription { get; set; }

        public bool? IsDeleted { get; set; }

        public int MinSelection { get; set; }
        public int MaxSelection { get; set; }

        public int ItemId { get; set; }

        public int? ItemtypeId { get; set; }

        public string Itemtype { get; set; }

        public int? ModifierGroupId { get; set; }

        public string UnitName { get; set; }


    }
}