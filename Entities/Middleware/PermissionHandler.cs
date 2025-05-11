using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Entities.ViewModel;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly PizzaShopContext _context;

    public PermissionHandler(IHttpContextAccessor httpContextAccessor, PizzaShopContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = _httpContextAccessor.HttpContext.User;

        if (user == null || !user.Identity.IsAuthenticated)
        {
            Console.WriteLine(" No authenticated user found.");
            return;
        }

        var roleClaim = user.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
        if (roleClaim == null)
        {
            Console.WriteLine(" Role claim not found.");
            return;
        }

        Console.WriteLine($" User Role: {roleClaim}");

        var userPermissions = await GetPermissionsForRoleAsync(roleClaim);

        Console.WriteLine($"Permissions found for role {roleClaim}: {userPermissions.Count}");

        foreach (var perm in userPermissions)
        {
            Console.WriteLine($"Fetched Permission: {perm.PermissionName} | CanView: {perm.CanView} | CanAddEdit: {perm.CanAddEdit} | CanDelete: {perm.CanDelete}");
        }

        bool hasPermission = requirement.Permission switch
        {
            string perm when perm.EndsWith("_View") => 
                userPermissions.Any(p => p.PermissionName == perm.Replace("_View", "") && (bool)p.CanView),
            string perm when perm.EndsWith("_Edit") => 
                userPermissions.Any(p => p.PermissionName == perm.Replace("_Edit", "") && (bool)p.CanAddEdit),
            string perm when perm.EndsWith("_Delete") => 
                userPermissions.Any(p => p.PermissionName == perm.Replace("_Delete", "") && (bool)p.CanDelete),
            _ => false
        };

        if (hasPermission)
        {
            Console.WriteLine($"Permission granted: {requirement.Permission}");
            context.Succeed(requirement);
        }
        else
        {
            Console.WriteLine($"Permission denied: {requirement.Permission}");
        }
    }

    private async Task<List<PermissionVM>> GetPermissionsForRoleAsync(string role)
    {
        return await _context.Permissions
            .Include(p => p.Role)
            .Where(p => p.Role != null && p.Role.RoleName == role)
            .Select(p => new PermissionVM
            {
                RoleId = p.RoleId,
                PermissionId = p.PermissionId,
                PermissionName = p.PermissionName,
                CanView = p.CanView,
                CanAddEdit = p.CanAddEdit,
                CanDelete = p.CanDelete
            })
            .ToListAsync();
    }
}