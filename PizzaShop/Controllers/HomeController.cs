﻿using System.Diagnostics;
using System.Security.Claims;
using DAL.Interfaces;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Entities.ViewModel;
using X.PagedList.Extensions;
using BLL.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using BLL.Interfaces;

namespace PizzaShop.Controllers;


public class HomeController : Controller
{
    // private readonly ILogger<HomeController> _logger;
    private readonly IUserService _userService;
    private readonly ITaxService _taxService;

    // ILogger<HomeController> logger
    public HomeController(IUserService userService, ITaxService taxService)
    {
        // _logger = logger;

        _userService = userService;
        _taxService = taxService;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Loginpage", "Authentication");
    }

    [HttpGet("AccessDenied")]
    public IActionResult AccessDenied()
    {
        TempData["Message"] = "You do not have permission to access this page!";
        TempData["MessageType"] = "error"; // Types: success, error, warning, info

        return View();
    }

    [HttpGet("NotFound")]
    public IActionResult NotFound()
    {
        return View();
    }

    public IActionResult TokenExpire()
    {

        return View();
    }


    // [Authorize(Policy = "ChefOnly")]
    // [Authorize(Roles = "Chef")]

    // [Authorize(Policy = "SuperAdminPolicy")]
    [Authorize]
    public IActionResult Dashboard()
    {
        Console.WriteLine("I am in Dashboard");
        return View();
    }

    // [Authorize]
    [Authorize(Policy = "UserViewPolicy")]
    public async Task<IActionResult> Userpage(UserFilterOptions filterOptions)
    {
        filterOptions.Page ??= 1;
        filterOptions.IsAsc ??= true;

        // Fetch paginated and filtered users from the service
        X.PagedList.IPagedList<UserViewModel> userViewModels = _userService.GetFilteredUsers(filterOptions);

        ViewBag.SortBy = filterOptions.SortBy;
        ViewBag.IsAsc = filterOptions.IsAsc;

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_UserList", userViewModels);
        }

        return View(userViewModels);
    }
    

    [Authorize(Policy = "MenuViewPolicy")]
    public IActionResult Menu()
    {
        return View();
    }

    [Authorize(Policy = "RoleandPermissionViewPolicy")]
    public IActionResult RoleandPermission()
    {
        var roles = _userService.GetAllRoles();
        return View(roles);
    }

    [Authorize(Policy = "TableAndSectionViewPolicy")]
    public IActionResult TableAndSection()
    {
        return View();
    }

    [Authorize(Policy = "TaxAndFeeViewPolicy")]
    public IActionResult TaxAndFee()
    {
        var taxtypes = _taxService.GetAllTaxTypes();
        ViewBag.Taxtypes = taxtypes.Select(r => new SelectListItem
        {
            Value = r.TaxTypeId.ToString(),
            Text = r.TaxTypeName
        }).ToList();
        return View();
    }


    [Authorize(Policy = "OrderViewPolicy")]
    public IActionResult Order()
    {
        return View();
    }


    // [Authorize(Policy = "CustomerViewPolicy")]
    public IActionResult Customer()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}