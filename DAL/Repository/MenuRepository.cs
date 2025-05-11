using  DAL.Interfaces;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace  DAL.Repository;

public class MenuRepository : IMenuRepository
{
    private readonly PizzaShopContext _context;

    public MenuRepository(PizzaShopContext context)
    {
        _context = context;
    }

    public IEnumerable<MenuCategory> GetAllCategories()
    {
        return _context.MenuCategories.Where(c => (bool)!c.IsDeleted).ToList();
    }

    public List<MenuItem> GetAllItems()
    {
        return _context.MenuItems.Where(i => (bool)!i.IsDeleted).ToList();
    }



    public MenuCategory GetCategoryById(int id)
    {
        return _context.MenuCategories.Include(c => c.MenuItems)
                                            .FirstOrDefault(c => c.CategoryId == id);
    }

    public string GetItemNameById(int itemId)
    {
        var itemName = (from item in _context.MenuItems
                        where item.ItemId == itemId
                        select item.ItemName).FirstOrDefault();
        return itemName;
    }

    public decimal GetModifierPriceById(int modifierId)
    {
        var modifierPrice = (from modifier in _context.MenuModifiers
                             where modifier.ModifierId == modifierId
                             select modifier.ModifierRate).FirstOrDefault();
        return (decimal)modifierPrice;
    }

    public int GetTotalModifierCount()
    {
        return _context.MenuModifiers.Count();  // Replace 'Modifiers' with your actual DbSet name
    }
    public int GetModifierQuantityById(int modifierId)
    {
        var modifierQuantity = (from modifier in _context.MenuModifiers
                                where modifier.ModifierId == modifierId
                                select modifier.Quantity).FirstOrDefault();
        return modifierQuantity;
    }

    public string GetModifierNameById(int modifierId)
    {
        var modifierName = (from modifier in _context.MenuModifiers
                            where modifier.ModifierId == modifierId
                            select modifier.ModifierName).FirstOrDefault();
        return modifierName;
    }

    public void AddCategory(MenuCategory category)
    {
        _context.MenuCategories.Add(category);
        Save();
    }

    public void UpdateCategory(MenuCategoryVM category)
    {
        var id = _context.MenuCategories.FirstOrDefault(u => u.CategoryId == category.CategoryId);
        id.CategoryDescription = category.CategoryDescription;
        id.CategoryName = category.CategoryName;
        Save();
    }

    public void DeleteCategory(int categoryId)
    {
        var existingCategory = _context.MenuCategories.FirstOrDefault(u => u.CategoryId == categoryId);
        if (existingCategory != null)
        {
            existingCategory.IsDeleted = true;
            Save();
        }
    }

    public bool IsItemFavourite(int itemId, int userId)
    {
        var favouriteItem = _context.Favourites.FirstOrDefault(f => f.ItemId == itemId && f.UserId == userId);
        return favouriteItem != null;
    }

    public List<MenuItem> GetItemsByCategoryId(int categoryId)
    {
        return _context.MenuItems.Where(i => i.CategoryId == categoryId && (bool)!i.IsDeleted).ToList();
    }

    public void AddMenuItem(MenuItem menuItem)
    {
        _context.MenuItems.Add(menuItem);
        _context.SaveChanges();

    }

    public void AddMenuItemModifierGroup(ItemModifierGroup menuitemmodifier)
    {
        _context.ItemModifierGroups.Add(menuitemmodifier);
        _context.SaveChanges();
    }


    public bool UpdateMenuItem(MenuItem menuItem)
    {
        var existingItem = _context.MenuItems.FirstOrDefault(m => m.ItemId == menuItem.ItemId);
        if (existingItem == null)
        {
            return false;
        }

        existingItem.ItemName = menuItem.ItemName;
        existingItem.CategoryId = menuItem.CategoryId;
        existingItem.ItemtypeId = menuItem.ItemtypeId;
        existingItem.Rate = menuItem.Rate;
        existingItem.Quantity = menuItem.Quantity;
        existingItem.UnitId = menuItem.UnitId;
        existingItem.IsAvailable = menuItem.IsAvailable;
        existingItem.TaxPercentage = menuItem.TaxPercentage;
        existingItem.ShortCode = menuItem.ShortCode;
        existingItem.Description = menuItem.Description;
        existingItem.TaxDefault = menuItem.TaxDefault;
        existingItem.CategoryPhoto = menuItem.CategoryPhoto;

        _context.SaveChanges();
        return true;
    }

    public void DeleteModifierGroupsByItemId(int itemId)
    {
        var existingModifiers = _context.ItemModifierGroups.Where(m => m.ItemId == itemId);
        _context.ItemModifierGroups.RemoveRange(existingModifiers);
        _context.SaveChanges();
    }




    public void DeleteItems(List<MenuItem> items)
    {
        Console.WriteLine(items);
        foreach (var p in items)
        {
            Console.WriteLine($"itemsId:{p.ItemId}");
        }

        // var item = _context.MenuItems.Where(i => items.Contains(i.ItemId)).ToList();

        foreach (var pr in items)
        {
            var existingitems = _context.MenuItems.FirstOrDefault(p => p.ItemId == pr.ItemId);
            existingitems.IsDeleted = true;
        }
        Save();
    }

    public void DeleteModifiers(List<MenuModifier> modifiers)
    {
        Console.WriteLine(modifiers);
        foreach (var p in modifiers)
        {
            Console.WriteLine($"itemsId:{p.ModifierId}");
        }

        // var item = _context.MenuItems.Where(i => items.Contains(i.ItemId)).ToList();

        foreach (var pr in modifiers)
        {
            var existingitems = _context.MenuModifiers.FirstOrDefault(p => p.ModifierId == pr.ModifierId);
            existingitems.IsDeleted = true;
        }
        Save();
    }


    public List<MenuModifier> GetModifiers()
    {
        return _context.MenuModifiers.ToList();
    }


    public IEnumerable<Itemtype> GetAllItemTypes()
    {
        return _context.Itemtypes.ToList();
    }


    public IEnumerable<Unit> GetAllUnits()
    {
        return _context.Units.ToList();
    }

    public IEnumerable<MenuModifierGroup> GetAllModifierGroups()
    {
        return _context.MenuModifierGroups.Where(m => (bool)!m.IsDeleted).ToList();
    }

    public MenuModifierGroup GetModifierGroupById(int id)
    {
        return _context.MenuModifierGroups.Include(c => c.MenuModifiers)
                                            .FirstOrDefault(c => c.ModifierGroupId == id);
    }

    // public string GetModifierGroupById(int modifierId)
    // {
    //     var groupName = (from modifier in _context.MenuModifiers
    //                     join groups in _context.MenuModifierGroups on modifier.ModifierGroupId equals groups.ModifierGroupId
    //                     where modifier.ModifierGroupId == modifierId
    //                     select groups.ModifierGroupName).FirstOrDefault();
    //     Console.WriteLine(groupName);
    //     return groupName;
    // }



    public List<MenuModifier> GetModifiersByModifierGroupId(int modifierGroupId)
    {
        // return _context.MenuModifiers.Where(i => i.ModifierGroupId == modifierGroupId).ToList();
        var result = (from cm in _context.CombineModifiers
                      join m in _context.MenuModifiers on cm.ModifierId equals m.ModifierId
                      where cm.ModifierGroupId == modifierGroupId && m.IsDeleted == false
                      orderby cm.CombineModifierId
                      select new MenuModifier
                      {
                          ModifierId = (int)cm.ModifierId,
                          ModifierGroupId = cm.ModifierGroupId,
                          ModifierName = m.ModifierName,
                          ModifierRate = m.ModifierRate,
                          UnitId = m.UnitId,
                          Quantity = m.Quantity,
                      }).ToList();

        return result;

    }

    public List<MenuModifierGroup> GetModifierGroupsByModifierId(int modifierId)
    {
        var result = (from cm in _context.CombineModifiers
                      join mg in _context.MenuModifierGroups on cm.ModifierGroupId equals mg.ModifierGroupId
                      where cm.ModifierId == modifierId
                      select new MenuModifierGroup
                      {
                          ModifierGroupId = mg.ModifierGroupId,
                          ModifierGroupName = mg.ModifierGroupName
                      }).ToList();

        return result;
    }

    public void RemoveCombinedModifierGroup(int modifierId, int groupId)
    {
        var entry = _context.CombineModifiers
                            .FirstOrDefault(cm => cm.ModifierId == modifierId && cm.ModifierGroupId == groupId);
        if (entry != null)
        {
            _context.CombineModifiers.Remove(entry);
            _context.SaveChanges();
        }
    }





    public string GetUnitById(int unitId)
    {
        var unitName = (from item in _context.MenuItems
                        join unit in _context.Units on item.UnitId equals unit.UnitId
                        select unit.UnitName).FirstOrDefault();
        return unitName;
    }

    public MenuItem GetItemById(int itemid)
    {
        return _context.MenuItems.FirstOrDefault(r => r.ItemId == itemid);
    }

    public string GetModifierGroupNameById(int modifiergroupid)
    {
        var grpName = (from modgrp in _context.MenuModifierGroups
                       where modgrp.ModifierGroupId == modifiergroupid
                       select modgrp.ModifierGroupName).FirstOrDefault();
        return grpName;

    }

    public MenuModifier GetModifierById(int modifierid)
    {
        return _context.MenuModifiers.FirstOrDefault(r => r.ModifierId == modifierid);
    }

    public List<ItemModifierGroup> GetItemModifier(int itemid)
    {
        return _context.ItemModifierGroups.Where(r => r.ItemId == itemid).ToList();
    }


    public void AddModifierGroup(MenuModifierGroup modifierGroup)
    {
        _context.MenuModifierGroups.Add(modifierGroup);
        Save();
    }

    public void AddCombinedModifierGroup(CombineModifier modifierGroup)
    {
        _context.CombineModifiers.Add(modifierGroup);
        Save();
    }

    public void AddModifier(MenuModifier modifier)
    {
        _context.MenuModifiers.Add(modifier);
        Save();
    }

    public void UpdateModifierGroup(MenuModifierGroupVM modifierGroup)
    {
        var id = _context.MenuModifierGroups.FirstOrDefault(u => u.ModifierGroupId == modifierGroup.ModifierGroupId);
        id.ModifierGroupName = modifierGroup.ModifierGroupName;
        id.ModifierGroupDecription = modifierGroup.ModifierGroupDecription;
        Save();
    }




    public void DeleteModifierGroup(MenuModifierGroup modifierGroup)
    {
        Console.WriteLine(modifierGroup);
        if (modifierGroup != null)
        {
            modifierGroup.IsDeleted = true;
            Save();
        }
    }

    public void UpdateModifier(MenuModifier modifier)
    {
        _context.MenuModifiers.Update(modifier);
        Save();
    }

    public void Save()
    {
        _context.SaveChanges();
    }



}