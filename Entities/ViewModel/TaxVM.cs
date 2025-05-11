using System.ComponentModel.DataAnnotations;
using Entities.Models;
using X.PagedList;

namespace Entities.ViewModel
{
    public class TaxVM
    {
        public List<Taxis> AllTaxes { get; set; }

        public IPagedList<Taxis>? Taxes { get; set; }

        public int TaxId { get; set; }

        public string TaxName { get; set; } = null!;

        public bool IsEnabled { get; set; }

        public bool IsDefault { get; set; }
        public int? TaxTypeId { get; set; }

        public string TaxTypeName { get; set; } = null!;

        public decimal TaxAmount { get; set; }

        public bool? IsDeleted { get; set; }


    }
}