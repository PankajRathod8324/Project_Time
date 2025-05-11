using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class TableStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public TimeOnly? OccupiedTime { get; set; }

    public virtual ICollection<Table> Tables { get; } = new List<Table>();
}
