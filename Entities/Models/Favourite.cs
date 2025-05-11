using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Favourite
{
    public int FavouritesId { get; set; }

    public int? ItemId { get; set; }

    public int? UserId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public virtual MenuItem? Item { get; set; }

    public virtual User? User { get; set; }
}
