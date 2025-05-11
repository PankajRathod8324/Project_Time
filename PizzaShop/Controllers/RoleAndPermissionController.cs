using System.Diagnostics;
using System.Security.Claims;
using BLL.Interfaces;
using  DAL.Interfaces;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PizzaShop.Controllers;


public class RoleAndPermissionController : Controller
{
    private readonly IPermissionService _roleService;

    public RoleAndPermissionController(IPermissionService roleService)
    {
        _roleService = roleService;
    }

    [Authorize(Policy = "RoleAndPermissionEditPolicy")]
    public IActionResult Permission(int id)
    {
        List<PermissionVM> permissionVMs = _roleService.GetPermissionsForRole(id);
        return View(permissionVMs);
    }

    [Authorize(Policy = "RoleAndPermissionEditPolicy")]
    [HttpPost]
    public IActionResult Permission([FromBody] List<PermissionVM> permissions)
    {
        if (permissions == null || permissions.Count == 0)
        {
            return BadRequest("No permissions received");
        }

        _roleService.UpdatePermissions(permissions);

        TempData["Message"] = "Successfully update permission.";
        TempData["MessageType"] = "success"; 

        return RedirectToAction("Permission", "RoleAndPermission");

    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}