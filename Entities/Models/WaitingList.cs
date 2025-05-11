using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class WaitingList
{
    public int WaitingListId { get; set; }

    public DateTime? WaitingTime { get; set; }

    public int SectionId { get; set; }

    public string Name { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int NoOfPerson { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<CustomerWaiting> CustomerWaitings { get; } = new List<CustomerWaiting>();

    public virtual Section Section { get; set; } = null!;
}
