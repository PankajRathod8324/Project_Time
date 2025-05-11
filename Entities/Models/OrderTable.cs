using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class OrderTable
{
    public int OrderTableId { get; set; }

    public int? OrderId { get; set; }

    public int? TableId { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Table? Table { get; set; }
}
