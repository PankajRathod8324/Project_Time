using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using DAL.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Entities.ViewModel;
using X.PagedList;
using X.PagedList.Extensions;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Azure.Core;
using BLL.Interfaces;

namespace BLL.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;

    private readonly IUserRepository _userRepository;

    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly String _imageFolderPath = "wwwroot/images/";
    public MenuService(IMenuRepository menuRepository, IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
    {
        _menuRepository = menuRepository;
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
    }




    public IEnumerable<MenuCategory> GetAllCategories()
    {
        return _menuRepository.GetAllCategories();
    }

    public MenuCategory GetCategoryById(int id)
    {
        return _menuRepository.GetCategoryById(id);
    }

    public void AddCategory(MenuCategory category)
    {
        _menuRepository.AddCategory(category);
    }

    public void UpdateCategory(MenuCategoryVM category)
    {
        _menuRepository.UpdateCategory(category);
    }

    public void DeleteCategory(int categoryId)
    {
        _menuRepository.DeleteCategory(categoryId);
    }

    public List<MenuItem> GetItemsByCategoryId(int categoryId)
    {
        return _menuRepository.GetItemsByCategoryId(categoryId);
    }

    public MenuCategoryVM GetEditItemViewModel(int itemId)
    {
        var item = GetItemById(itemId);
        if (item == null)
        {
            return null; // Return null if item not found
        }

        var itemModifiers = GetItemModifier(item.ItemId);

        return new MenuCategoryVM
        {
            ItemId = item.ItemId,
            CategoryId = item.CategoryId,
            ItemName = item.ItemName,
            ItemtypeId = item.ItemtypeId,
            Rate = item.Rate,
            Quantity = item.Quantity,
            UnitId = item.UnitId,
            IsAvailable = item.IsAvailable ?? false,
            TaxDefault = item.TaxDefault,
            TaxPercentage = item.TaxPercentage,
            ShortCode = item.ShortCode,
            Description = item.Description,
            ModifierGroupId = item.ModifierGroupId,
            ModifierGroupIds = itemModifiers.Select(m => new ItemModifierVM
            {
                ItemId = m.ItemId,
                ModifierGroupId = m.ModifierGroupId,
                ModifierGroupName = m.ModifierGroupId != 0 ? GetModifierGroupNameById(m.ModifierGroupId) : "No GroupName",
                MinSelection = m.MinSelection,
                MaxSelection = m.MaxSelection,
                MenuModifiers = GetModifiersByModifierGroupId(m.ModifierGroupId)
                    .Select(mod => new ModifierVM
                    {
                        ModifierId = mod.ModifierId,
                        ModifierName = mod.ModifierName,
                        ModifierRate = (decimal)mod.ModifierRate,
                    }).ToList(),
                MenuModifierGroupItem = GetModifiersByModifierGroupId(m.ModifierGroupId)
                    .Select(mod => new MenuModifierGroupVM
                    {
                        ModifierId = mod.ModifierId,
                        ModifierName = mod.ModifierName,
                        ModifierRate = (decimal)mod.ModifierRate,
                    }).ToList()
            }).ToList()
        };
    }




    public List<ItemModifierGroup> GetItemModifier(int itemid)
    {
        return _menuRepository.GetItemModifier(itemid);
    }

    public string GetModifierGroupNameById(int modifiergroupid)
    {
        return _menuRepository.GetModifierGroupNameById(modifiergroupid);
    }

    public void AddMenuItem(MenuItem menuItem)
    {
        _menuRepository.AddMenuItem(menuItem);
    }

    public void AddMenuItemModifierGroup(ItemModifierGroup menuitemmodifier)
    {
        _menuRepository.AddMenuItemModifierGroup(menuitemmodifier);
    }

    public bool UpdateMenuItem(MenuItem menuItem)
    {
        return _menuRepository.UpdateMenuItem(menuItem);
    }

    public void UpdateMenuItemModifierGroups(int itemId, List<ItemModifierGroup> modifierGroups)
    {
        // Step 1: Remove existing modifier groups for the item
        _menuRepository.DeleteModifierGroupsByItemId(itemId);

        // Step 2: Add new modifier groups
        if (modifierGroups != null && modifierGroups.Any())
        {
            foreach (var modifierGroup in modifierGroups)
            {
                var newModifierGroup = new ItemModifierGroup
                {
                    ItemId = itemId,
                    ModifierGroupId = modifierGroup.ModifierGroupId,
                    MinSelection = modifierGroup.MinSelection,
                    MaxSelection = modifierGroup.MaxSelection
                };

                _menuRepository.AddMenuItemModifierGroup(newModifierGroup);
            }
        }
    }


    public void DeleteItem(List<MenuItem> items)
    {
        _menuRepository.DeleteItems(items);
    }

    public void DeleteModifiers(List<MenuModifier> modifiers)
    {
        _menuRepository.DeleteModifiers(modifiers);
    }


    public IEnumerable<Itemtype> GetAllItemTypes()
    {
        return _menuRepository.GetAllItemTypes();
    }

    public IEnumerable<Unit> GetAllUnits()
    {
        return _menuRepository.GetAllUnits();
    }

    public IEnumerable<MenuModifierGroup> GetAllModifierGroups()
    {
        return _menuRepository.GetAllModifierGroups();
    }
    public MenuModifierGroup GetModifierGroupById(int id)
    {
        return _menuRepository.GetModifierGroupById(id);
    }

    // public string GetModifierNameById(int modifierId, MenuModifierGroupVM modifierGroups)
    // {
    //     return _menuRepository.GetModifierNameById(modifierId, modifierGroups);
    // }

    public IPagedList<MenuModifierGroupVM> GetModifiers(UserFilterOptions filterOptions)
    {

        var modifier = _menuRepository.GetModifiers().AsQueryable();
        if (!string.IsNullOrEmpty(filterOptions.Search))
        {
            string searchLower = filterOptions.Search.ToLower();
            modifier = modifier.Where(u => u.ModifierName.ToLower().Contains(searchLower) ||
                                     u.ModifierRate.ToString().ToLower().Contains(searchLower));
        }

        // Get total count and handle page size dynamically
        int totalTables = modifier.Count();
        int pageSize = filterOptions.PageSize > 0 ? Math.Min(filterOptions.PageSize, totalTables) : 10; // Default 10

        var modifierItems = modifier.Select(item => new MenuModifierGroupVM
        {
            ModifierGroupId = item.ModifierGroupId ?? 0, // Avoid null exception
            ModifierId = item.ModifierId,
            ModifierName = item.ModifierName,
            ModifierRate = item.ModifierRate,
            UnitId = item.UnitId,
            Quantity = item.Quantity,
            ModifierDecription = item.ModifierDecription,
            UnitName = item.UnitId.HasValue ? (_menuRepository.GetUnitById(item.UnitId.Value) ?? "No Unit") : "No Unit"
        }).ToPagedList(filterOptions.Page.Value, filterOptions.PageSize);


        return modifierItems;
    }
    public string GetUnitById(int unitId)
    {
        return _menuRepository.GetUnitById(unitId);
    }

    public MenuModifier GetModifierById(int modifierid)
    {
        return _menuRepository.GetModifierById(modifierid);
    }

    public int GetTotalModifierCount()
    {
        return _menuRepository.GetTotalModifierCount();
    }


    public void AddCombinedModifierGroup(CombineModifier modifierGroup)
    {
        _menuRepository.AddCombinedModifierGroup(modifierGroup);
    }
    public MenuItem GetItemById(int itemid)
    {
        return _menuRepository.GetItemById(itemid);
    }
    public List<MenuModifier> GetModifiersByModifierGroupId(int modifierGroupId)
    {
        return _menuRepository.GetModifiersByModifierGroupId(modifierGroupId);
    }

    public List<MenuModifierGroup> GetModifierGroupsByModifierId(int modifierId)
    {
        return _menuRepository.GetModifierGroupsByModifierId(modifierId);
    }

    public void RemoveCombinedModifierGroup(int modifierId, int groupId)
    {
        _menuRepository.RemoveCombinedModifierGroup(modifierId, groupId);
    }

    public void AddModifierGroup(MenuModifierGroup modifierGroup)
    {
        _menuRepository.AddModifierGroup(modifierGroup);
    }

    public void UpdateModifierGroup(MenuModifierGroupVM modifierGroup)
    {
        _menuRepository.UpdateModifierGroup(modifierGroup);
    }

    public void DeleteModifierGroup(MenuModifierGroup modifierGroup)
    {
        _menuRepository.DeleteModifierGroup(modifierGroup);
    }

    public void AddModifier(MenuModifier modifier)
    {
        _menuRepository.AddModifier(modifier);
    }

    public void UpdateModifier(MenuModifier modifier)
    {
        _menuRepository.UpdateModifier(modifier);
    }

    public ServiceResult AddMenuItem(JsonObject menuItemData)
    {
        try
        {
            // Step 1: Map JSON to ViewModel directly inside the method
            var menuItem = new MenuItem
            {
                ItemName = menuItemData["ItemName"]?.ToString(),
                CategoryId = TryParseInt(menuItemData["CategoryId"]),
                ItemtypeId = TryParseInt(menuItemData["ItemtypeId"]),
                Rate = TryParseDecimal(menuItemData["Rate"]),
                Quantity = TryParseInt(menuItemData["Quantity"]),
                UnitId = TryParseInt(menuItemData["UnitId"]),
                IsAvailable = TryParseBool(menuItemData["IsAvailable"]),
                TaxPercentage = TryParseDecimal(menuItemData["TaxPercentage"]),
                ShortCode = menuItemData["ShortCode"]?.ToString(),
                Description = menuItemData["Description"]?.ToString(),
                TaxDefault = TryParseBool(menuItemData["TaxDefault"]),
                CategoryPhoto = null // Temporary, will be set after processing
            };

            // Step 2: Validate
            if (string.IsNullOrEmpty(menuItem.ItemName))
            {
                return new ServiceResult { Success = false, Message = "Item Name cannot be empty." };
            }

            // Step 3: Process Image (if provided)
            if (!string.IsNullOrEmpty(menuItemData["ItemPhoto"]?.ToString()))
            {
                var base64String = menuItemData["ItemPhoto"].ToString().Split(',')[1];
                byte[] imageBytes = Convert.FromBase64String(base64String);
                var uniqueFileName = Guid.NewGuid().ToString() + ".jpg";
                var filePath = Path.Combine(_imageFolderPath, uniqueFileName);

                Directory.CreateDirectory(_imageFolderPath); // Ensure the directory exists

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    stream.Write(imageBytes, 0, imageBytes.Length);
                }

                menuItem.CategoryPhoto = "/images/" + uniqueFileName;
            }

            // Step 4: Save Menu Item
            _menuRepository.AddMenuItem(menuItem);

            // Step 5: Save Modifier Groups (if present)
            if (menuItemData.ContainsKey("ModifierGroupIds") && menuItemData["ModifierGroupIds"] != null)
            {
                var modifierGroups = JsonConvert.DeserializeObject<List<ItemModifierGroup>>(menuItemData["ModifierGroupIds"].ToString());

                if (modifierGroups.Any())
                {
                    foreach (var modifierGroup in modifierGroups)
                    {
                        var menuItemModifier = new ItemModifierGroup
                        {
                            ItemId = menuItem.ItemId,
                            ModifierGroupId = modifierGroup.ModifierGroupId,
                            MinSelection = modifierGroup.MinSelection,
                            MaxSelection = modifierGroup.MaxSelection
                        };
                        _menuRepository.AddMenuItemModifierGroup(menuItemModifier);
                    }

                }
            }

            return new ServiceResult { Success = true, Message = "Menu Item Added Successfully!" };
        }
        catch (Exception ex)
        {
            return new ServiceResult { Success = false, Message = "An error occurred: " + ex.Message };
        }
    }


    public MenuCategoryVM GetEditMenuItemViewModel(int itemId)
    {
        var item = _menuRepository.GetItemById(itemId);
        if (item == null) return null;

        var itemModifiers = _menuRepository.GetItemModifier(item.ItemId);

        return new MenuCategoryVM
        {
            ItemId = item.ItemId,
            CategoryId = item.CategoryId,
            ItemName = item.ItemName,
            ItemtypeId = item.ItemtypeId,
            Rate = item.Rate,
            Quantity = item.Quantity,
            UnitId = item.UnitId,
            IsAvailable = item.IsAvailable ?? false,
            TaxDefault = item.TaxDefault,
            TaxPercentage = item.TaxPercentage,
            ShortCode = item.ShortCode,
            Description = item.Description,
            ModifierGroupId = item.ModifierGroupId,
            ItemPhoto = item.CategoryPhoto,
            ModifierGroupIds = itemModifiers.Select(m => new ItemModifierVM
            {
                ItemId = m.ItemId,
                ModifierGroupId = m.ModifierGroupId,
                ModifierGroupName = m.ModifierGroupId != 0 ? _menuRepository.GetModifierGroupNameById(m.ModifierGroupId) : "No Group Name",
                MinSelection = m.MinSelection,
                MaxSelection = m.MaxSelection,
                MenuModifiers = _menuRepository.GetModifiersByModifierGroupId(m.ModifierGroupId)
                    .Select(mod => new ModifierVM
                    {
                        ModifierId = mod.ModifierId,
                        ModifierName = mod.ModifierName,
                        ModifierRate = (decimal)mod.ModifierRate,
                    }).ToList(),
                MenuModifierGroupItem = _menuRepository.GetModifiersByModifierGroupId(m.ModifierGroupId)
                    .Select(mod => new MenuModifierGroupVM
                    {
                        ModifierId = mod.ModifierId,
                        ModifierName = mod.ModifierName,
                        ModifierRate = (decimal)mod.ModifierRate,
                    }).ToList()
            }).ToList()
        };
    }

    public ServiceResult UpdateMenuItemFromJson(JsonObject menuItemData, string imageFolderPath)
    {
        try
        {
            // Extract fields
            int itemId = TryParseInt(menuItemData["ItemId"]);
            string itemName = menuItemData["ItemName"]?.ToString();
            int categoryId = TryParseInt(menuItemData["CategoryId"]);
            int itemTypeId = TryParseInt(menuItemData["ItemtypeId"]);
            decimal rate = TryParseDecimal(menuItemData["Rate"]);
            int quantity = TryParseInt(menuItemData["Quantity"]);
            int unitId = TryParseInt(menuItemData["UnitId"]);
            bool isAvailable = TryParseBool(menuItemData["IsAvailable"]);
            decimal taxPercentage = TryParseDecimal(menuItemData["TaxPercentage"]);
            string shortCode = menuItemData["ShortCode"]?.ToString();
            string description = menuItemData["Description"]?.ToString();
            bool taxDefault = TryParseBool(menuItemData["TaxDefault"]);
            string itemPhoto = menuItemData["ItemPhoto"]?.ToString();

            // Process Image Upload
            string photoPath = SaveBase64Image(itemPhoto, imageFolderPath);

            // Parse Modifier Groups
            List<ItemModifierGroup> modifierGroups = ParseModifierGroups(menuItemData);

            // Update Menu Item
            var menuItem = new MenuItem
            {
                ItemId = itemId,
                CategoryId = categoryId,
                ItemName = itemName,
                ItemtypeId = itemTypeId,
                Rate = rate,
                Quantity = quantity,
                UnitId = unitId,
                IsAvailable = isAvailable,
                TaxPercentage = taxPercentage,
                ShortCode = shortCode,
                Description = description,
                TaxDefault = taxDefault,
                CategoryPhoto = photoPath
            };

            bool updateSuccess = _menuRepository.UpdateMenuItem(menuItem);
            if (!updateSuccess)
            {
                return new ServiceResult { Success = false, Message = "Failed to update Menu Item." };
            }
            // Update Modifier Groups
            // Step 1: Remove existing modifier groups for the item
            _menuRepository.DeleteModifierGroupsByItemId(itemId);

            // Step 2: Add new modifier groups
            if (modifierGroups != null && modifierGroups.Any())
            {
                foreach (var modifierGroup in modifierGroups)
                {
                    var newModifierGroup = new ItemModifierGroup
                    {
                        ItemId = itemId,
                        ModifierGroupId = modifierGroup.ModifierGroupId,
                        MinSelection = modifierGroup.MinSelection,
                        MaxSelection = modifierGroup.MaxSelection
                    };

                    _menuRepository.AddMenuItemModifierGroup(newModifierGroup);
                }
            }

            return new ServiceResult { Success = true, Message = "Menu Item Updated Successfully!" };
        }
        catch (Exception ex)
        {
            return new ServiceResult { Success = false, Message = $"Error: {ex.Message}" };
        }
    }
    public int? AddModifierGroupWithModifiers(MenuModifierGroupVM model)
    {
        try
        {
            Console.WriteLine("Received Data:");
            Console.WriteLine("Modifier Group Name: " + model.ModifierGroupName);
            Console.WriteLine("Modifier Group Description: " + model.ModifierGroupDecription);
            Console.WriteLine("Selected Modifier IDs: " + (model.ModifierIds != null ? string.Join(", ", model.ModifierIds) : "None"));

            // Step 1️⃣ Save Modifier Group
            var modifierGroup = new MenuModifierGroup
            {
                ModifierGroupName = model.ModifierGroupName,
                ModifierGroupDecription = model.ModifierGroupDecription
            };

            _menuRepository.AddModifierGroup(modifierGroup);
            Console.WriteLine("Saved Modifier Group ID: " + modifierGroup.ModifierGroupId);

            // Step 2️⃣ Associate Combined Modifiers
            if (model.ModifierIds != null && model.ModifierIds.Any())
            {
                foreach (var combinedModifierId in model.ModifierIds)
                {
                    var existingModifier = _menuRepository.GetModifierById(combinedModifierId);
                    if (existingModifier != null)
                    {
                        var combinedModifier = new CombineModifier
                        {
                            ModifierGroupId = modifierGroup.ModifierGroupId,
                            ModifierId = combinedModifierId
                        };
                        _menuRepository.AddCombinedModifierGroup(combinedModifier);
                        Console.WriteLine($"Linked Combined Modifier ID {combinedModifierId} to Modifier Group ID {modifierGroup.ModifierGroupId}");
                    }
                    else
                    {
                        Console.WriteLine($"Combined Modifier with ID {combinedModifierId} not found, skipping.");
                    }
                }
            }

            return modifierGroup.ModifierGroupId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while adding Modifier Group: {ex.Message}");
            return null;
        }
    }

    public bool UpdateModifierGroupWithModifiers(MenuModifierGroupVM model)
    {
        try
        {
            Console.WriteLine("Editing Modifier Group ID:" + model.ModifierGroupId);
            Console.WriteLine("New Name:" + model.ModifierGroupName);
            Console.WriteLine("New Description:" + model.ModifierGroupDecription);

            var existingGroup = _menuRepository.GetModifierGroupById(model.ModifierGroupId);
            if (existingGroup == null)
            {
                Console.WriteLine("Modifier Group not found.");
                return false;
            }

            // Update Modifier Group Details
            existingGroup.ModifierGroupName = model.ModifierGroupName;
            existingGroup.ModifierGroupDecription = model.ModifierGroupDecription;
            _menuRepository.UpdateModifierGroup(model);

            // Fetch Existing Modifier Associations
            var existingModifierIds = _menuRepository.GetModifiersByModifierGroupId(model.ModifierGroupId)
                                                     .Select(m => m.ModifierId)
                                                     .ToList();

            // Determine Modifiers to Remove & Remove them
            var modifiersToRemove = existingModifierIds.Except(model.ModifierIds).ToList();
            foreach (var modifierId in modifiersToRemove)
            {
                _menuRepository.RemoveCombinedModifierGroup(modifierId, existingGroup.ModifierGroupId);
            }

            // Determine Modifiers to Add & Add them
            var modifiersToAdd = model.ModifierIds.Except(existingModifierIds).ToList();
            foreach (var modifierId in modifiersToAdd)
            {
                var combinedModifier = new CombineModifier
                {
                    ModifierId = modifierId,
                    ModifierGroupId = model.ModifierGroupId
                };
                _menuRepository.AddCombinedModifierGroup(combinedModifier);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while updating Modifier Group: {ex.Message}");
            return false;
        }
    }





    // Helper Method: Save Base64 Image
    private string SaveBase64Image(string base64Image, string folderPath)
    {
        if (string.IsNullOrWhiteSpace(base64Image)) return null;

        try
        {
            var base64String = base64Image.Split(',')[1];
            byte[] imageBytes = Convert.FromBase64String(base64String);

            var uniqueFileName = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(folderPath, uniqueFileName);
            Directory.CreateDirectory(folderPath);

            File.WriteAllBytes(filePath, imageBytes);
            return "/images/" + uniqueFileName;
        }
        catch
        {
            return null;
        }
    }

    // Helper Method: Parse Modifier Groups
    private List<ItemModifierGroup> ParseModifierGroups(JsonObject menuItemData)
    {
        if (menuItemData.ContainsKey("ModifierGroupIds") && menuItemData["ModifierGroupIds"] != null)
        {
            return JsonConvert.DeserializeObject<List<ItemModifierGroup>>(menuItemData["ModifierGroupIds"].ToString());
        }
        return new List<ItemModifierGroup>();
    }


    private static int TryParseInt(object value)
    {
        return int.TryParse(value?.ToString(), out int result) ? result : 0;
    }

    private static decimal TryParseDecimal(object value)
    {
        return decimal.TryParse(value?.ToString(), out decimal result) ? result : 0;
    }

    private static bool TryParseBool(object value)
    {
        return bool.TryParse(value?.ToString(), out bool result) && result;
    }

    public List<MenuCategoryVM> GetItemByCategoryId(int categoryId, string search)
    {
        // var user = _httpContextAccessor.HttpContext?.Request.Cookies["Email"];
        string useremail = _httpContextAccessor.HttpContext?.Request.Cookies["Email"];
        var user = _userRepository.GetUserByEmail(useremail);

        var item = new List<MenuItem>();
        if (categoryId == 0)
        {

            item = _menuRepository.GetAllItems();
            if (!string.IsNullOrEmpty(search))
            {
                string searchLower = search.ToLower();
                item = item.Where(u => u.ItemName.ToLower().Contains(searchLower)).ToList();
            }
        }
        else
        {
            item = _menuRepository.GetItemsByCategoryId(categoryId);
            if (!string.IsNullOrEmpty(search))
            {
                string searchLower = search.ToLower();
                item = item.Where(u => u.ItemName.ToLower().Contains(searchLower)).ToList();
            }
        }


        var itemvm = item.Select(item => new MenuCategoryVM
        {
            ItemId = item.ItemId,
            ItemName = item.ItemName,
            UnitId = item.UnitId,
            CategoryId = item.CategoryId,
            ItemtypeId = item.ItemtypeId,
            Rate = item.Rate,
            Quantity = item.Quantity,
            IsAvailable = (bool)item.IsAvailable,
            TaxDefault = item.TaxDefault,
            TaxPercentage = item.TaxPercentage,
            ShortCode = item.ShortCode,
            Description = item.Description,
            ItemPhoto = item.CategoryPhoto,
            IsFavourite = _menuRepository.IsItemFavourite(item.ItemId, user.UserId) ? true : false,
        }).ToList();
        return itemvm;


    }


    public IPagedList<MenuCategoryVM> getFilteredMenuItems(int categoryId, UserFilterOptions filterOptions)
    {
        var menuItems = _menuRepository.GetItemsByCategoryId(categoryId).AsQueryable();

        if (!string.IsNullOrEmpty(filterOptions.Search))
        {
            string searchLower = filterOptions.Search.ToLower();
            menuItems = menuItems.Where(u => u.ItemName.ToLower().Contains(searchLower) ||
                                     u.Itemtype.ItemType1.ToLower().Contains(searchLower));
        }

        // Get total count and handle page size dynamically
        int totalItems = menuItems.Count();
        int pageSize = filterOptions.PageSize > 0 ? Math.Min(filterOptions.PageSize, totalItems) : 10; // Default 10

        var paginateditems = menuItems
           .Select(item => new MenuCategoryVM
           {
               ItemId = item.ItemId,
               ItemName = item.ItemName,
               UnitId = item.UnitId,
               CategoryId = item.CategoryId,
               ItemtypeId = item.ItemtypeId,
               Rate = item.Rate,
               Quantity = item.Quantity,
               IsAvailable = true,
               TaxDefault = true,
               TaxPercentage = item.TaxPercentage,
               ShortCode = item.ShortCode,
               Description = item.Description,
               ItemPhoto = item.CategoryPhoto

               // UnitName =  item.UnitId.HasValue ? _menuService.GetUnitById(item.UnitId.Value) : "No Unit"

           }).ToPagedList(filterOptions.Page.Value, filterOptions.PageSize);


        return paginateditems;

    }
    public IPagedList<MenuModifierGroupVM> getFilteredMenuModifiers(int groupId, UserFilterOptions filterOptions)
    {
        var menuModifiers = _menuRepository.GetModifiersByModifierGroupId(groupId).AsQueryable();

        if (!string.IsNullOrEmpty(filterOptions.Search))
        {
            string searchLower = filterOptions.Search.ToLower();
            menuModifiers = menuModifiers.Where(u => u.ModifierName.ToLower().Contains(searchLower));
        }

        // Get total count and handle page size dynamically
        int totalItems = menuModifiers.Count();
        int pageSize = filterOptions.PageSize > 0 ? Math.Min(filterOptions.PageSize, totalItems) : 10; // Default 10

        var paginatedmodifiers = menuModifiers
           .Select(item => new MenuModifierGroupVM
           {
               ModifierId = item.ModifierId,
               ModifierName = item.ModifierName,
               UnitId = item.UnitId,
               ModifierRate = item.ModifierRate,
               Quantity = item.Quantity,
               IsDeleted = item.IsDeleted,
               ModifierDecription = item.ModifierDecription,
               UnitName = item.UnitId.HasValue ? _menuRepository.GetUnitById(item.UnitId.Value) : "No Unit"

           }).ToPagedList(filterOptions.Page.Value, filterOptions.PageSize);


        return paginatedmodifiers;

    }
}