using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class CustomerWaiting
{
    public int CustomerWaitingsId { get; set; }

    public int? CustomerId { get; set; }

    public int? WaitingListId { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual WaitingList? WaitingList { get; set; }
}
