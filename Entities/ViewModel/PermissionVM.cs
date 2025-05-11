namespace Entities.ViewModel;
public class PermissionVM
{
    public string Role { get; set; }

    public int? RoleId {get; set;}
    public string PermissionName { get; set; }

    public int PermissionId { get; set;}
    public bool? CanView { get; set; }
    public bool? CanAddEdit { get; set; }
    public bool? CanDelete { get; set; }
}