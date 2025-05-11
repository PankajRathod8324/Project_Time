using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Taxis
{
    public int TaxId { get; set; }

    public string TaxName { get; set; } = null!;

    public bool? IsEnabled { get; set; }

    public bool IsDefault { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public int? TaxTypeId { get; set; }

    public decimal TaxAmount { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<OrderTax> OrderTaxes { get; } = new List<OrderTax>();

    public virtual TaxType? TaxType { get; set; }
}
