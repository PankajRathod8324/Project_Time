using  DAL.Interfaces;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace  DAL.Repository;

public class PermissionRepository : IPermissionRepository
{
    private readonly PizzaShopContext _context;
    public PermissionRepository(PizzaShopContext context)
    {
        _context = context;
    }
    public int GetRoleIdByRoleName(string rolename)
    {
        var userid = (from role in _context.Roles
                      where role.RoleName == rolename
                      select role.RoleId).FirstOrDefault();
        return userid;
    }
    public List<PermissionVM> GetAllPermissionsByroleId(int roleId)
    {
        return _context.Permissions
            .Where(p => p.RoleId == roleId)
            .OrderBy(p => p.PermissionId)
            .Select(p => new PermissionVM
            {
                RoleId = p.RoleId,
                PermissionId = p.PermissionId,
                PermissionName = p.PermissionName,
                CanView = p.CanView,
                CanAddEdit = p.CanAddEdit,
                CanDelete = p.CanDelete
            })
            .ToList();
    }
    public bool UpdateRolePermissions(List<PermissionVM> permissions)
    {
        Console.WriteLine(permissions);
        foreach (var p in permissions)
        {
            Console.WriteLine($"PermissionId:{p.PermissionId}, " +
                                $"CanAddEdit: {p.CanAddEdit}, " +
                                $"CanView: {p.CanView}, " +
                                $"CanDelete: {p.CanDelete} ");
        }
        foreach (var pr in permissions)
        {
            var existingPermissions = _context.Permissions.FirstOrDefault(p => p.PermissionId == pr.PermissionId);

            if (existingPermissions != null)
            {
                existingPermissions.CanView = pr.CanView;
                existingPermissions.CanAddEdit = pr.CanAddEdit;
                existingPermissions.CanDelete = pr.CanDelete;
            }

        }

        return _context.SaveChanges() > 0;
    }
    public Permission GetPermissionByName(string name)
    {
        return _context.Permissions.FirstOrDefault(p => p.PermissionName == name);
    }
}