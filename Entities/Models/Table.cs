using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Table
{
    public int TableId { get; set; }

    public string TableName { get; set; } = null!;

    public int? SectionId { get; set; }

    public int Capacity { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public int? StatusId { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? OccupiedTime { get; set; }

    public virtual ICollection<CustomerTable> CustomerTables { get; } = new List<CustomerTable>();

    public virtual ICollection<Customer> Customers { get; } = new List<Customer>();

    public virtual ICollection<OrderTable> OrderTables { get; } = new List<OrderTable>();

    public virtual Section? Section { get; set; }

    public virtual TableStatus? Status { get; set; }
}
