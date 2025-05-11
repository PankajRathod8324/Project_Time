using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json.Nodes;
using BLL.Interfaces;
using DAL.Interfaces;
using DAL.Repository;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PizzaShop.Controllers;


public class TableAndSectionController : Controller
{
    private readonly ITableAndSectionService _tableandsectionService;

    public TableAndSectionController(ITableAndSectionService tableandsectionService)
    {
        _tableandsectionService = tableandsectionService;
    }


    [Authorize(Policy = "TableAndSectionViewPolicy")]
    public IActionResult Section()
    {
        var sections = _tableandsectionService.GetAllSection();
        ViewBag.Sections = sections.Select(r => new SelectListItem
        {
            Value = r.SectionId.ToString(),
            Text = r.SectionName
        }).ToList();

        var statuses = _tableandsectionService.GetAllStatus();
        ViewBag.Statuses = statuses.Select(r => new SelectListItem
        {
            Value = r.StatusId.ToString(),
            Text = r.StatusName
        }).ToList();

        var sectionvm = new SectionVM
        {
            AllSections = sections
        };
        return PartialView("_SectionPV", sectionvm);
    }

    [Authorize(Policy = "TableAndSectionEditPolicy")]
    public IActionResult MenuTable(int sectionId, UserFilterOptions filterOptions)
    {
        filterOptions.Page ??= 1;
        filterOptions.PageSize = filterOptions.PageSize != 0 ? filterOptions.PageSize : 10; // Default page size

        ViewBag.PageSize = filterOptions.PageSize;

        var sections = _tableandsectionService.GetAllSection();
        ViewBag.Sections = sections.Select(r => new SelectListItem
        {
            Value = r.SectionId.ToString(),
            Text = r.SectionName
        }).ToList();

        var statuses = _tableandsectionService.GetAllStatus();
        ViewBag.Statuses = statuses.Select(r => new SelectListItem
        {
            Value = r.StatusId.ToString(),
            Text = r.StatusName
        }).ToList();

        var tablevm = _tableandsectionService.getFilteredMenuTables(sectionId, filterOptions);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_TablePV", tablevm);
        }
        return View(tablevm);
    }




    [Authorize(Policy = "TableAndSectionEditPolicy")]
    [HttpPost]
    public IActionResult AddSection(SectionVM section)
    {
        _tableandsectionService.AddSection(section);
        return RedirectToAction("TableAndSection", "Home");
    }

    [Authorize(Policy = "TableAndSectionEditPolicy")]
    [HttpPost]
    public IActionResult EditSection(SectionVM model)
    {
        Console.WriteLine("Edit name:" + model.SectionName);

        _tableandsectionService.EditSection(model);
        return RedirectToAction("TableAndSection", "Home");
    }

    [Authorize(Policy = "TableAndSectionDeletePolicy")]
    [HttpPost]
    public IActionResult DeleteSection(int sectionId)
    {
        Console.WriteLine("tHIS IS iD: " + sectionId);
        var table = _tableandsectionService.GetTablesBySectionId(sectionId);
        foreach (var item in table)
        {
            if (item.StatusId != 1)
            {
                TempData["Message"] = "Section cannot be deleted because it has tables are Occupied.";
                TempData["MessageType"] = "warning";
                 return RedirectToAction("TableAndSection", "Home");
            }
        }
        _tableandsectionService.DeleteSection(sectionId);
        TempData["Message"] = "Section deleted successfully!";
        TempData["MessageType"] = "success";
        return RedirectToAction("TableAndSection", "Home");
    }

    [HttpPost]
    public IActionResult AddTable([FromBody] JsonObject tableData)
    {
        if (tableData == null)
        {
            return BadRequest("Invalid JSON format. Could not parse data.");
        }

        Console.WriteLine("Raw JSON received: " + tableData.ToString());

        // {"TableName":"GT3","SectionId":"3","Capacity":"5","StatusId":"1"}
        // Extract individual values safely
        string tableName = tableData["TableName"]?.ToString();
        int sectionId = TryParseInt(tableData["SectionId"]);
        int capacity = TryParseInt(tableData["Capacity"]);
        int statusId = TryParseInt(tableData["StatusId"]);


        // Step 1: Save Menu Item
        var table = new Table
        {
            TableName = tableName,
            SectionId = sectionId,
            Capacity = capacity,
            StatusId = statusId
        };

        _tableandsectionService.AddTable(table);

        return Json(new { success = true, message = "Menu Item Added Successfully!" });
    }



    [Authorize(Policy = "TableAndSectionEditPolicy")]
    [HttpGet]
    public IActionResult EditTable(int tableId, int pagenumber)
    {
        // Fetch Categories, Item Types, Units, and Modifier Groups
        var sections = _tableandsectionService.GetAllSection();
        ViewBag.Sections = sections.Select(r => new SelectListItem
        {
            Value = r.SectionId.ToString(),
            Text = r.SectionName
        }).ToList();

        var statuses = _tableandsectionService.GetAllStatus();
        ViewBag.Statuses = statuses.Select(r => new SelectListItem
        {
            Value = r.StatusId.ToString(),
            Text = r.StatusName
        }).ToList();

        // Fetch Item Data

        var table = _tableandsectionService.GetTableById(tableId);
        if (table == null)
        {
            return NotFound(); // Return a 404 if the item doesn't exist
        }
        ViewBag.Page = pagenumber;
        Console.WriteLine("In Edit PageNumber: " + pagenumber);

        Console.WriteLine("I am IN Water");

        // var modifiers = _menuService.GetModifiersByModifierGroupId((int)item.ModifierGroupId);

        var itemvm = new SectionVM
        {
            SectionId = (int)table.SectionId,
            TableName = table.TableName,
            Capacity = table.Capacity,
            StatusId = table.StatusId,
        };



        return PartialView("_EditTablePV", itemvm);
    }



    [Authorize(Policy = "TableAndSectionEditPolicy")]
    [HttpPost]
    public IActionResult EditTable([FromBody] JsonObject tableData)
    {
        if (tableData == null)
        {
            return BadRequest("Invalid JSON format. Could not parse data.");
        }

        Console.WriteLine("Raw JSON received: " + tableData.ToString());

        // Extract Item ID
        int tableId = TryParseInt(tableData["TableId"]);
        if (tableId <= 0)
        {
            return BadRequest("Invalid Item ID.");
        }

        // Extract individual fields
        string tableName = tableData["TableName"]?.ToString();
        int sectionId = TryParseInt(tableData["SectionId"]);
        int capacity = TryParseInt(tableData["Capacity"]);
        int statusId = TryParseInt(tableData["StatusId"]);

        // Parse Modifier Groups JSON array safely

        // Step 1: Update Menu Item
        var table = new Table
        {
            TableId = tableId,
            TableName = tableName,
            SectionId = sectionId,
            Capacity = capacity,
            StatusId = statusId
        };

        bool updateSuccess = _tableandsectionService.UpdateTable(table);

        if (!updateSuccess)
        {
            return BadRequest("Failed to update Table.");
        }

        TempData["Message"] = "Table updated successfully!";
        TempData["MessageType"] = "success";

        return Json(new { success = true, message = "Table Updated Successfully!" });
    }

    [Authorize(Policy = "TableAndSectionDeletePolicy")]
    [HttpPost]
    public IActionResult DeleteTable([FromBody] List<Table> tables)
    {

        Console.WriteLine("HEEJNJKFNJN");
        if (tables == null || tables.Count == 0)
        {
            return BadRequest("No items received");
        }

        Console.WriteLine("Updatedjfnldnfledf");
        Console.WriteLine(tables.Count);

        _tableandsectionService.DeleteTables(tables);

        TempData["Message"] = "Successfully Delete Table.";
        TempData["MessageType"] = "success"; // Types: success, error, warning, info



        return RedirectToAction("MenuTable", "TableAndSection");

    }

    private int TryParseInt(object value)
    {
        return int.TryParse(value?.ToString(), out int result) ? result : 0;
    }







    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}