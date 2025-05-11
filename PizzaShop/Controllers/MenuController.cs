using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json.Nodes;
using BLL.Interfaces;
using  DAL.Interfaces;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

// using PizzaShop.Models;
using X.PagedList.Extensions;

namespace PizzaShop.Controllers;


public class MenuController : Controller
{
    private readonly IMenuService _menuService;

    private readonly String _imageFolderPath = "wwwroot/images/";

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [Authorize(Policy = "MenuEditPolicy")]
    public IActionResult MenuItem(int categoryId, UserFilterOptions filterOptions)
    {
        filterOptions.Page ??= 1;
        filterOptions.PageSize = filterOptions.PageSize != 0 ? filterOptions.PageSize : 10; // Default page size

        ViewBag.PageSize = filterOptions.PageSize;

        // Populate ViewBag with dropdown data
        PopulateViewBags();

        var menuitemvm = _menuService.getFilteredMenuItems(categoryId, filterOptions);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_MenuItemPV", menuitemvm);
        }

        return View(menuitemvm);
    }

    [HttpPost]
    public IActionResult AddMenuItem([FromBody] JsonObject menuItemData)
    {
        if (menuItemData == null)
        {
            return BadRequest("Invalid JSON format.");
        }

        var result = _menuService.AddMenuItem(menuItemData);

        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

        return Json(new { success = true, message = result.Message });
    }


    [Authorize(Policy = "MenuEditPolicy")]
    public IActionResult EditMenuItem(int itemId, int pageNumber)
    {
        var itemvm = _menuService.GetEditMenuItemViewModel(itemId);
        if (itemvm == null)
        {
            return NotFound();
        }

        ViewBag.Page = pageNumber;
        PopulateViewBags();

        return PartialView("_EditItemPV", itemvm);
    }

    [Authorize(Policy = "MenuEditPolicy")]
    [HttpPost]
    public IActionResult EditMenuItem([FromBody] JsonObject menuItemData)
    {
        if (menuItemData == null)
        {
            return BadRequest("Invalid JSON format. Could not parse data.");
        }

        Console.WriteLine("Raw JSON received: " + menuItemData.ToString());

        // Extract and validate item ID
        int itemId = TryParseInt(menuItemData["ItemId"]);
        if (itemId <= 0)
        {
            return BadRequest("Invalid Item ID.");
        }

        // Delegate parsing and updating logic to the service layer
        var result = _menuService.UpdateMenuItemFromJson(menuItemData, _imageFolderPath);

        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

        TempData["Message"] = "Item updated successfully!";
        TempData["MessageType"] = "success";

        return Json(new { success = true, message = "Menu Item Updated Successfully!" });
    }


    // Extracted Method to Populate ViewBag
    private void PopulateViewBags()
    {
        var categories = _menuService.GetAllCategories();
        var itemTypes = _menuService.GetAllItemTypes();
        var units = _menuService.GetAllUnits();
        var modifierGroups = _menuService.GetAllModifierGroups();

        ViewBag.Categories = categories.Select(r => new SelectListItem { Value = r.CategoryId.ToString(), Text = r.CategoryName }).ToList();
        ViewBag.Itemtypes = itemTypes.Select(r => new SelectListItem { Value = r.ItemtypeId.ToString(), Text = r.ItemType1 }).ToList();
        ViewBag.Units = units.Select(r => new SelectListItem { Value = r.UnitId.ToString(), Text = r.UnitName }).ToList();
        ViewBag.ModifierGroups = modifierGroups.Select(r => new SelectListItem { Value = r.ModifierGroupId.ToString(), Text = r.ModifierGroupName }).ToList();
    }



    [Authorize(Policy = "MenuViewPolicy")]

    public IActionResult MenuCategory()
    {
        PopulateViewBags();
        var categories = _menuService.GetAllCategories();

        var categoryvm = new MenuCategoryVM
        {
            menuCategories = categories
        };

        return PartialView("_MenuCategoryPV", categoryvm);
    }

    [Authorize(Policy = "MenuViewPolicy")]
    public IActionResult MenuModifier(int groupId, UserFilterOptions filterOptions)
    {
        filterOptions.Page ??= 1;
        filterOptions.PageSize = filterOptions.PageSize != 0 ? filterOptions.PageSize : 10; // Default page size

        var units = _menuService.GetAllUnits(); // Fetch Units from the database
        var unitSelectList = units.Select(r => new SelectListItem
        {
            Value = r.UnitId.ToString(),
            Text = r.UnitName
        }).ToList();
        ViewBag.Units = unitSelectList;

        var modifiergroups = _menuService.GetAllModifierGroups(); // Fetch Units from the database
        var modifiergroupSelectList = modifiergroups.Select(r => new SelectListItem
        {
            Value = r.ModifierGroupId.ToString(),
            Text = r.ModifierGroupName
        }).ToList();
        ViewBag.ModifierGroups = modifiergroupSelectList;


        Console.WriteLine("Click this : " + groupId);
        filterOptions.Page ??= 1;

        X.PagedList.IPagedList<MenuModifierGroupVM> menumodifiervm = _menuService.getFilteredMenuModifiers(groupId, filterOptions);

        return PartialView("_MenuModifierPV", menumodifiervm);
    }

    [Authorize]
    public IActionResult GetAllModifier(UserFilterOptions filterOptions)
    {
        filterOptions.Page ??= 1;
        filterOptions.PageSize = filterOptions.PageSize != 0 ? filterOptions.PageSize : 10; // Default page size

        var modifiers = _menuService.GetModifiers(filterOptions);

        return PartialView("_MenuModifierByModifierGroup", modifiers);
    }



    [Authorize]
    public IActionResult GetEditAllModifier(int modifierGroupId, UserFilterOptions filterOptions)
    {
        filterOptions.Page ??= 1;
        filterOptions.PageSize = filterOptions.PageSize != 0 ? filterOptions.PageSize : 10; // Default page size

        var pagedModifiers = _menuService.GetModifiers(filterOptions);
        int totalItems = _menuService.GetTotalModifierCount(); // Implement this in your service

        var associatedModifierIds = _menuService.GetModifiersByModifierGroupId(modifierGroupId)
            .Select(m => m.ModifierId)
            .ToList();

        var menuModifierGroup = new MenuModifierGroupVM
        {
            ModifierGroupId = modifierGroupId,
            Modifier = pagedModifiers,
            ModifierIds = associatedModifierIds
        };

        var viewModel = new PaginatedViewModel<MenuModifierGroupVM>
        {
            Data = menuModifierGroup,
            PageSize = filterOptions.PageSize,
            PageNumber = filterOptions.Page.Value,
            TotalItemCount = totalItems
        };

        return PartialView("_EditModifierGroupTable", viewModel);
    }


    [Authorize]
    public IActionResult GetModifiersByGroup(int groupId, UserFilterOptions filterOptions)
    {
        var modifiers = _menuService.GetModifiersByModifierGroupId(groupId);
        var groupName = _menuService.GetModifierGroupById(groupId);

        var modifieritems = modifiers
           .Select(item => new MenuModifierGroupVM
           {
               ModifierGroupId = (int)item.ModifierGroupId,
               ModifierName = item.ModifierName,
               ModifierRate = item.ModifierRate,
               UnitId = item.UnitId,
               Quantity = item.Quantity,
               ModifierDecription = item.ModifierDecription,
               UnitName = item.UnitId.HasValue ? _menuService.GetUnitById(item.UnitId.Value) : "No Unit"

           }).ToList();
        MenuModifierGroupVM modifiervm = new MenuModifierGroupVM
        {
            menuModifiers = modifieritems,
            ModifierGroupId = groupId,
            ModifierGroupName = groupName.ModifierGroupName


            // UnitName = modifiers.UnitId.HasValue ? _menuService.GetUnitById(modifiers.UnitId.Value) : "No Units"
        };


        // var groupName = _menuService.GetModifierNameById(groupId, modifiervm);
        return PartialView("_ModifierList", modifiervm);
    }

    [Authorize]
    public IActionResult GetModifiersByGroupEdit(int groupId, UserFilterOptions filterOptions)
    {
        var modifiers = _menuService.GetModifiersByModifierGroupId(groupId);
        var groupName = _menuService.GetModifierGroupById(groupId);

        var modifieritems = modifiers.Select(item => new MenuModifierGroupVM
        {
            ModifierGroupId = (int)item.ModifierGroupId,
            ModifierName = item.ModifierName,
            ModifierRate = item.ModifierRate,
            UnitId = item.UnitId,
            Quantity = item.Quantity,
            ModifierDecription = item.ModifierDecription,
            UnitName = item.UnitId.HasValue ? _menuService.GetUnitById(item.UnitId.Value) : "No Unit"
        }).ToList();

        // Creating the ItemModifierVM instance
        ItemModifierVM modifieritemvm = new ItemModifierVM
        {
            ModifierGroupId = groupId,
            ModifierGroupName = groupName.ModifierGroupName,
            MenuModifierGroupItem = modifieritems
        };

        return PartialView("_EditModifierList", modifieritemvm);
    }


    public IActionResult GetModifiersGroupByItem(int groupId, UserFilterOptions filterOptions, int itemId)
    {
        var modifiers = _menuService.GetModifiersByModifierGroupId(groupId);
        var groupName = _menuService.GetModifierGroupById(groupId);
        var combinemodifier = _menuService.GetItemModifier(itemId);

        var modifieritems = modifiers
           .Select(item => new MenuModifierGroupVM
           {
               ModifierName = item.ModifierName,
               ModifierRate = item.ModifierRate,
           }).ToList();
        MenuModifierGroupVM modifiervm = new MenuModifierGroupVM
        {
            menuModifiers = modifieritems,
            ModifierGroupId = groupId,
            ModifierGroupName = groupName.ModifierGroupName,
            itemModifierGroups = combinemodifier.Select(modifier => new ItemModifierVM
            {
                ItemId = modifier.ItemId,
                ModifierGroupId = modifier.ModifierGroupId,
                MaxSelection = modifier.MaxSelection,
                MinSelection = modifier.MinSelection
            }).ToList(),
        };
        return Json(modifiervm);
    }

    [Authorize(Policy = "MenuViewPolicy")]
    public IActionResult MenuModifierGroup()
    {
        var units = _menuService.GetAllUnits(); // Fetch Units from the database
        var unitSelectList = units.Select(r => new SelectListItem
        {
            Value = r.UnitId.ToString(),
            Text = r.UnitName
        }).ToList();
        ViewBag.Units = unitSelectList;

        var modifiergroups = _menuService.GetAllModifierGroups(); // Fetch Units from the database
        var modifiergroupSelectList = modifiergroups.Select(r => new SelectListItem
        {
            Value = r.ModifierGroupId.ToString(),
            Text = r.ModifierGroupName
        }).ToList();
        ViewBag.ModifierGroups = modifiergroupSelectList;


        var modifierGroups = _menuService.GetAllModifierGroups();

        var modifierGroupvm = new MenuModifierGroupVM
        {
            menuModifierGroups = modifierGroups
        };

        return PartialView("_MenuModifierGroupPV", modifierGroupvm);
    }


    [Authorize(Policy = "MenuEditPolicy")]
    [HttpPost]
    public IActionResult AddMenuCategory(MenuCategory category)
    {
        if (ModelState.IsValid)
        {
            _menuService.AddCategory(category);
            TempData["Message"] = "Category added successfully!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Menu", "Home");
        }
        return RedirectToAction("Menu", "Home");
    }

    [Authorize(Policy = "MenuEditPolicy")]
    [HttpPost]
    public IActionResult EditCategory(MenuCategoryVM model)
    {
        Console.WriteLine("Edit name:" + model.CategoryName);
        Console.WriteLine("Edit name:" + model.CategoryId);
        Console.WriteLine("Edit name:" + model.CategoryDescription);
        TempData["Message"] = "Category updated successfully!";
        TempData["MessageType"] = "info";
        _menuService.UpdateCategory(model);
        return RedirectToAction("Menu", "Home");
    }


    [Authorize(Policy = "MenuDeletePolicy")]
    [HttpPost]
    public IActionResult DeleteCategory(int id)
    {
        _menuService.DeleteCategory(id);
        TempData["Message"] = "Category deleted successfully!";
        TempData["MessageType"] = "success";
        return RedirectToAction("Menu", "Home");
    }


    [Authorize(Policy = "MenuDeletePolicy")]
    public IActionResult DeleteItem([FromBody] List<MenuItem> items)
    {

        Console.WriteLine("HEEJNJKFNJN");
        if (items == null || items.Count == 0)
        {
            return BadRequest("No items received");
        }

        Console.WriteLine("Updatedjfnldnfledf");
        Console.WriteLine(items.Count);

        _menuService.DeleteItem(items);

        TempData["Message"] = "Successfully Delete Item.";
        TempData["MessageType"] = "success";
        return RedirectToAction("Menu", "Home");

    }


    private int TryParseInt(object value)
    {
        return int.TryParse(value?.ToString(), out int result) ? result : 0;
    }

    private decimal TryParseDecimal(object value)
    {
        return decimal.TryParse(value?.ToString(), out decimal result) ? result : 0;
    }

    private bool TryParseBool(object value)
    {
        return bool.TryParse(value?.ToString(), out bool result) ? result : false;
    }

    [HttpGet]
    public IActionResult GetModifierGroup(int id)
    {
        Console.WriteLine($"Fetching Modifier Group for ID: {id}");

        var group = _menuService.GetModifierGroupById(id);
        if (group == null)
        {
            Console.WriteLine("❌ Modifier Group not found!");
            return NotFound("Modifier Group not found.");
        }

        var modifiers = _menuService.GetModifiersByModifierGroupId(id);

        Console.WriteLine($"✅ Found {modifiers.Count()} Modifiers in Group {id}");

        var viewModel = new MenuModifierGroupVM
        {
            ModifierGroupId = group.ModifierGroupId,
            ModifierGroupName = group.ModifierGroupName,
            ModifierGroupDecription = group.ModifierGroupDecription,
            Modifiers = modifiers.Select(m => new ModifierVM
            {
                ModifierId = m.ModifierId,
                ModifierName = m.ModifierName
            }).ToList()
        };

        return PartialView("_EditModifierGroupPV", viewModel); // Return Partial View

    }

    [Authorize(Policy = "MenuEditPolicy")]
    [HttpPost]
    public IActionResult AddMenuModifierGroup([FromBody] MenuModifierGroupVM model)
    {
        if (model == null)
        {
            return BadRequest(new { success = false, message = "Invalid data received." });
        }

        var modifierGroupId = _menuService.AddModifierGroupWithModifiers(model);

        if (modifierGroupId == null)
        {
            return BadRequest(new { success = false, message = "Failed to add Modifier Group." });
        }

        var filterOptions = new UserFilterOptions { Page = 1, PageSize = 10 };
        return MenuModifier((int)modifierGroupId, filterOptions);
    }

    [Authorize(Policy = "MenuEditPolicy")]
    [HttpPost]
    public IActionResult EditModifierGroup([FromBody] MenuModifierGroupVM model)
    {
        if (model == null)
        {
            return Json(new { success = false, message = "Invalid data received." });
        }

        var result = _menuService.UpdateModifierGroupWithModifiers(model);

        if (!result)
        {
            return Json(new { success = false, message = "Failed to update Modifier Group." });
        }

        var filterOptions = new UserFilterOptions { Page = 1, PageSize = 10 };
        return MenuModifier(model.ModifierGroupId, filterOptions);
    }


    [Authorize(Policy = "MenuDeletePolicy")]
    [HttpPost]
    public IActionResult DeleteModifierGroup(int groupId)
    {
        Console.WriteLine("This is ID to delete: " + groupId);

        // Get all modifier groups sorted by ID
        var allGroups = _menuService.GetAllModifierGroups()
                                    .OrderBy(mg => mg.ModifierGroupId)
                                    .ToList();

        // Find index of the current group
        var currentIndex = allGroups.FindIndex(mg => mg.ModifierGroupId == groupId);

        // Find the previous group's ID (if available)
        int? previousGroupId = null;
        if (currentIndex > 0) // Ensure there's a previous group
        {
            previousGroupId = allGroups[currentIndex - 1].ModifierGroupId;
        }

        // Delete the current modifier group
        var modifierGroup = _menuService.GetModifierGroupById(groupId);
        if (modifierGroup != null)
        {
            _menuService.DeleteModifierGroup(modifierGroup);
        }

        // Set success message
        TempData["Message"] = "Modifier Group deleted successfully!";
        TempData["MessageType"] = "error";

        Console.WriteLine("Previous Group ID: " + (previousGroupId.HasValue ? previousGroupId.ToString() : "None"));

        // Redirect or return data
        var filterOptions = new UserFilterOptions
        {
            Page = 1,
            PageSize = 10 // Default values, adjust as needed
        };
        return MenuModifier((int)previousGroupId, filterOptions);// Pass previousGroupId to reload view with it
    }

    [Authorize(Policy = "MenuEditPolicy")]
    public IActionResult AddMenuModifier(MenuModifierGroupVM menuModifier)
    {
        Console.WriteLine(ModelState.IsValid);
        // Console.WriteLine("--------------Add Modifier" + menuModifier.ModifierGroupId);


        var units = _menuService.GetAllUnits(); // Fetch Units from the database
        var unitSelectList = units.Select(r => new SelectListItem
        {
            Value = r.UnitId.ToString(),
            Text = r.UnitName
        }).ToList();
        ViewBag.Units = unitSelectList;

        var modifiergroups = _menuService.GetAllModifierGroups(); // Fetch Units from the database
        var modifiergroupSelectList = modifiergroups.Select(r => new SelectListItem
        {
            Value = r.ModifierGroupId.ToString(),
            Text = r.ModifierGroupName
        }).ToList();
        ViewBag.ModifierGroups = modifiergroupSelectList;


        var newmenumodifier = new MenuModifier
        {
            ModifierName = menuModifier.ModifierName,
            ModifierRate = menuModifier.ModifierRate,
            Quantity = menuModifier.Quantity,
            UnitId = menuModifier.UnitId,
            ModifierDecription = menuModifier.ModifierDecription

        };

        _menuService.AddModifier(newmenumodifier);

        // Add multiple Modifier Groups
        if (menuModifier.ModifierGroupIds != null && menuModifier.ModifierGroupIds.Any())
        {
            var uniqueGroupIds = menuModifier.ModifierGroupIds.Distinct().ToList();
            foreach (var groupId in uniqueGroupIds)
            {
                var combinedModifier = new CombineModifier
                {
                    ModifierId = newmenumodifier.ModifierId,
                    ModifierGroupId = groupId
                };
                _menuService.AddCombinedModifierGroup(combinedModifier);
            }
        }

        Console.WriteLine("--------------Add Modifier END");
        TempData["Message"] = "Modifier added successfully!";
        TempData["MessageType"] = "success";



        return Json(new { success = true, message = "Modifier Added Successfully!" });
    }

    public IActionResult EditMenuModifier(int modifierid)
    {
        Console.WriteLine(modifierid);

        // Fetch Units from the database
        var units = _menuService.GetAllUnits();
        ViewBag.Units = units.Select(r => new SelectListItem
        {
            Value = r.UnitId.ToString(),
            Text = r.UnitName
        }).ToList();

        // Fetch Modifier Groups from the database
        var modifierGroups = _menuService.GetAllModifierGroups();
        ViewBag.ModifierGroups = modifierGroups.Select(r => new SelectListItem
        {
            Value = r.ModifierGroupId.ToString(),
            Text = r.ModifierGroupName
        }).ToList();

        // Fetch the modifier item
        var modifier = _menuService.GetModifierById(modifierid);
        if (modifier == null)
        {
            return NotFound();
        }

        // Fetch associated modifier group IDs (multiple)
        var assignedModifierGroups = _menuService.GetModifierGroupsByModifierId(modifierid);

        var itemvm = new MenuModifierGroupVM
        {
            ModifierId = modifier.ModifierId,
            ModifierGroupIds = assignedModifierGroups.Select(mg => mg.ModifierGroupId).ToList(), // Multiple selection
            ModifierName = modifier.ModifierName,
            ModifierRate = modifier.ModifierRate,
            UnitId = modifier.UnitId,
            Quantity = modifier.Quantity,
            ModifierDecription = modifier.ModifierDecription,
            UnitName = modifier.UnitId.HasValue ? _menuService.GetUnitById(modifier.UnitId.Value) : "No Unit"
        };

        return PartialView("_EditModifierPV", itemvm);
    }

    [HttpPost]
    [Authorize(Policy = "MenuEditPolicy")]
    public IActionResult EditMenuModifier([FromBody] MenuModifierGroupVM menuModifier)
    {
        if (menuModifier == null)
        {
            return Json(new { success = false, message = "Invalid request data." });
        }

        Console.WriteLine(ModelState.IsValid);

        // Fetch the existing modifier
        var existingModifier = _menuService.GetModifierById(menuModifier.ModifierId);
        if (existingModifier == null)
        {
            return Json(new { success = false, message = "Modifier not found." });
        }

        // Update the modifier properties
        existingModifier.ModifierName = menuModifier.ModifierName;
        existingModifier.ModifierRate = menuModifier.ModifierRate;
        existingModifier.Quantity = menuModifier.Quantity;
        existingModifier.UnitId = menuModifier.UnitId;
        existingModifier.ModifierDecription = menuModifier.ModifierDecription;

        _menuService.UpdateModifier(existingModifier);

        // Fetch existing assigned modifier groups
        var existingModifierGroups = _menuService.GetModifierGroupsByModifierId(existingModifier.ModifierId)
                                                  .Select(mg => mg.ModifierGroupId)
                                                  .ToList();

        var newModifierGroups = menuModifier.ModifierGroupIds ?? new List<int>();

        // Find groups to remove (exist in old but not in new)
        var groupsToRemove = existingModifierGroups.Except(newModifierGroups).ToList();
        // Find groups to add (exist in new but not in old)
        var groupsToAdd = newModifierGroups.Except(existingModifierGroups).ToList();

        // Remove modifier groups that are no longer selected
        foreach (var groupId in groupsToRemove)
        {
            _menuService.RemoveCombinedModifierGroup(existingModifier.ModifierId, groupId);
        }

        // Add new modifier groups that were selected
        foreach (var groupId in groupsToAdd)
        {
            var combinedModifier = new CombineModifier
            {
                ModifierId = existingModifier.ModifierId,
                ModifierGroupId = groupId
            };
            _menuService.AddCombinedModifierGroup(combinedModifier);
        }
        TempData["Message"] = "Modifier updated successfully!";
        TempData["MessageType"] = "success";

        return Json(new { success = true, message = "Modifier updated successfully!" });
    }

    public IActionResult DeleteModifier([FromBody] List<MenuModifier> modifiers)
    {
        if (modifiers == null || modifiers.Count == 0)
        {
            return BadRequest("No modifiers received");
        }

        _menuService.DeleteModifiers(modifiers);

        TempData["Message"] = "Successfully Delete Item.";
        TempData["MessageType"] = "success";
        return RedirectToAction("Menu", "Home");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}