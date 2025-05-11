using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Permission
{
    public int PermissionId { get; set; }

    public int? RoleId { get; set; }

    public string PermissionName { get; set; } = null!;

    public bool? CanView { get; set; }

    public bool? CanAddEdit { get; set; }

    public bool? CanDelete { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public virtual Role? Role { get; set; }
}
