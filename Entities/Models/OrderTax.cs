using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class OrderTax
{
    public int OrderTaxId { get; set; }

    public int? OrderId { get; set; }

    public int? TaxId { get; set; }

    public decimal? TaxRate { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Taxis? Tax { get; set; }
}
