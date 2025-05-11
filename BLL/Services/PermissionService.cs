using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using  DAL.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Entities.ViewModel;
using BLL.Interfaces;

namespace BLL.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;

    private readonly IUserService _userService;
    public PermissionService(IPermissionRepository permissionRepository, IUserService userService)
    {
        _permissionRepository = permissionRepository;
        _userService = userService;
    }

    public int GetRoleIdByRoleName(string rolename)
    {
        return _permissionRepository.GetRoleIdByRoleName(rolename);
    }

    public Permission GetPermissionByName(string name)
    {
        return _permissionRepository.GetPermissionByName(name);
    }

    public bool UpdatePermissions(List<PermissionVM> permissions)
    {
        var permissionVMs = permissions.Select(p => new PermissionVM
        {
            PermissionId = p.PermissionId,
            PermissionName = p.PermissionName,
            CanView = p.CanView,
            CanAddEdit = p.CanAddEdit,
            CanDelete = p.CanDelete
        }).ToList();

        return _permissionRepository.UpdateRolePermissions(permissionVMs);
    }

    public List<PermissionVM> GetPermissionsForRole(int roleId)
    {
        var permissions = _permissionRepository.GetAllPermissionsByroleId(roleId);

        return permissions.Select(p => new PermissionVM
        {
            PermissionName = p.PermissionName,
            PermissionId = p.PermissionId,
            Role = p.RoleId.HasValue ? _userService.GetRoleNameById(p.RoleId.Value) : "No Role",
            CanView = p.CanView,
            CanAddEdit = p.CanAddEdit,
            CanDelete = p.CanDelete
        }).ToList();
    }
}