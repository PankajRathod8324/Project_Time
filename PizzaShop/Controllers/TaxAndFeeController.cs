using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json.Nodes;
using BLL.Interfaces;
using  DAL.Interfaces;
using  DAL.Repository;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PizzaShop.Controllers;


public class TaxAndFeeController : Controller
{
    private readonly ITaxService _taxService;

    public TaxAndFeeController(ITaxService taxService)
    {
        _taxService = taxService;
    }


    [Authorize(Policy = "TaxAndFeeViewPolicy")]
    public IActionResult Tax(UserFilterOptions filterOptions)
    {
        filterOptions.Page ??= 1;
        filterOptions.PageSize = filterOptions.PageSize != 0 ? filterOptions.PageSize : 10; // Default page size

        ViewBag.PageSize = filterOptions.PageSize;

        var taxtypes = _taxService.GetAllTaxTypes();
        ViewBag.Taxtypes = taxtypes.Select(r => new SelectListItem
        {
            Value = r.TaxTypeId.ToString(),
            Text = r.TaxTypeName
        }).ToList();


        var tablevm = _taxService.getFilteredTaxes(filterOptions);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_TaxesPV", tablevm);
        }
        return View(tablevm);
    }

    [Authorize(Policy = "TaxAndFeeEditPolicy")]
    public IActionResult AddPermission()
    {
        return Ok();
    }

    [Authorize(Policy = "TaxAndFeeEditPolicy")]
    [HttpPost]
    public IActionResult AddTax([FromBody] JsonObject taxData)
    {
        if (taxData["TaxName"]?.ToString() == "")
        {
            return BadRequest("Invalid JSON format. Could not parse data.");
        }

        Console.WriteLine("Raw JSON received: " + taxData.ToString());

        // {"TableName":"GT3","SectionId":"3","Capacity":"5","StatusId":"1"}
        // Extract individual values safely
        string taxName = (string)taxData["TaxName"];
        bool isDefault = (bool)taxData["IsDefault"];
        bool isEnabled = (bool)taxData["IsEnabled"];
        int taxTypeId = TryParseInt(taxData["taxTypeId"]);
        decimal taxAmount = TryParseDecimal(taxData["TaxAmount"]);


        // Step 1: Save Menu Item
        var tax = new Taxis
        {
            TaxName = taxName,
            IsEnabled = isEnabled,
            IsDefault = isDefault,
            TaxTypeId = taxTypeId,
            TaxAmount = taxAmount
        };

        _taxService.AddTax(tax);

        return Json(new { success = true, message = "Menu Item Added Successfully!" });
    }

    [Authorize(Policy = "TaxAndFeeEditPolicy")]
    [HttpGet]
    public IActionResult EditTax(int taxId, int pagenumber)
    {
        // Fetch Categories, Item Types, Units, and Modifier Groups
        var taxtypes = _taxService.GetAllTaxTypes();
        ViewBag.Taxtypes = taxtypes.Select(r => new SelectListItem
        {
            Value = r.TaxTypeId.ToString(),
            Text = r.TaxTypeName
        }).ToList();


        // Fetch Item Data

        var tax = _taxService.GetTaxById(taxId);
        if (tax == null)
        {
            return NotFound(); // Return a 404 if the item doesn't exist
        }
        ViewBag.Page = pagenumber;
        Console.WriteLine("In Edit PageNumber: " + pagenumber);

        Console.WriteLine("I am IN Water");

        // var modifiers = _menuService.GetModifiersByModifierGroupId((int)item.ModifierGroupId);

        var taxvm = new TaxVM
        {
            TaxName = tax.TaxName,
            IsEnabled = (bool)tax.IsEnabled,
            IsDefault = tax.IsDefault,
            TaxTypeId = tax.TaxTypeId,
            TaxAmount = tax.TaxAmount
        };



        return PartialView("_EditTaxPV", taxvm);
    }


    [Authorize(Policy = "TaxAndFeeEditPolicy")]
    [HttpPost]
    public IActionResult EditTax([FromBody] JsonObject taxData)
    {
        if (taxData == null)
        {
            return BadRequest("Invalid JSON format. Could not parse data.");
        }

        Console.WriteLine("Raw JSON received: " + taxData.ToString());

        // Extract Item ID
        int taxId = TryParseInt(taxData["TaxId"]);
        if (taxId <= 0)
        {
            return BadRequest("Invalid Item ID.");
        }

        // Extract individual fields
        string taxName = (string)taxData["TaxName"];
        bool isDefault = (bool)taxData["IsDefault"];
        bool isEnabled = (bool)taxData["IsEnabled"];
        int taxTypeId = TryParseInt(taxData["taxTypeId"]);
        decimal taxAmount = TryParseDecimal(taxData["TaxAmount"]);

        // Parse Modifier Groups JSON array safely

        // Step 1: Update Menu Item
        var tax = new Taxis
        {
            TaxId = taxId,
            TaxName = taxName,
            IsEnabled = isEnabled,
            IsDefault = isDefault,
            TaxTypeId = taxTypeId,
            TaxAmount = taxAmount
        };

        bool updateSuccess = _taxService.UpdateTax(tax);

        if (!updateSuccess)
        {
            return BadRequest("Failed to update Table.");
        }


        return Json(new { success = true, message = "Tax Updated Successfully!" });
    }

    [Authorize(Policy = "TaxAndFeeDeletePolicy")]
    [HttpPost]
    public IActionResult DeletePermission(int taxId, int pagenumber)
    {
        ViewBag.Page = pagenumber;
        Console.WriteLine("In Edit PageNumber: " + pagenumber);
        return Ok();
    }

    [Authorize(Policy = "TaxAndFeeDeletePolicy")]
    [HttpPost]
    public IActionResult DeleteTax(int taxId, int pagenumber)
    {
        var tax = _taxService.GetTaxById(taxId);
        _taxService.DeleteTax(tax);
        return Json(new { success = true, message = "Tax Updated Successfully!" });
    }




    private static int TryParseInt(object value)
    {
        return int.TryParse(value?.ToString(), out int result) ? result : 0;
    }

    private static decimal TryParseDecimal(object value)
    {
        return decimal.TryParse(value?.ToString(), out decimal result) ? result : 0;
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}