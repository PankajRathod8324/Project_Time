using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class TaxType
{
    public int TaxTypeId { get; set; }

    public string TaxTypeName { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public virtual ICollection<Taxis> Taxes { get; } = new List<Taxis>();
}
