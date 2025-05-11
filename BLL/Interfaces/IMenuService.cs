using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.AspNetCore.Http;
using X.PagedList;

namespace  BLL.Interfaces;
public interface IMenuService
{
  IEnumerable<MenuCategory> GetAllCategories();

  MenuCategory GetCategoryById(int id);

  void AddCategory(MenuCategory category);

  void UpdateCategory(MenuCategoryVM category);

  int GetTotalModifierCount();

  void DeleteCategory(int categoryId);
  List<MenuItem> GetItemsByCategoryId(int categoryId);

  List<MenuCategoryVM> GetItemByCategoryId(int categoryId, string search);

  List<ItemModifierGroup> GetItemModifier(int itemid);

  MenuCategoryVM GetEditItemViewModel(int itemId);
  void AddMenuItem(MenuItem menuItem);

  void AddMenuItemModifierGroup(ItemModifierGroup menuitemmodifier);

  bool UpdateMenuItem(MenuItem menuItem);

  public MenuModifier GetModifierById(int modifierid);

  public void AddCombinedModifierGroup(CombineModifier modifierGroup);

  public void DeleteItem(List<MenuItem> items);

  public IEnumerable<Itemtype> GetAllItemTypes();

  IPagedList<MenuModifierGroupVM> GetModifiers(UserFilterOptions filterOptions);

  public IEnumerable<Unit> GetAllUnits();

  public IEnumerable<MenuModifierGroup> GetAllModifierGroups();

  public IPagedList<MenuCategoryVM> getFilteredMenuItems(int categoryId, UserFilterOptions filterOptions);

  public IPagedList<MenuModifierGroupVM> getFilteredMenuModifiers(int groupId, UserFilterOptions filterOptions);
  MenuModifierGroup GetModifierGroupById(int id);

  public string GetModifierGroupNameById(int modifiergroupid);

  

  //  string GetModifierNameById(int modifierId, MenuModifierGroupVM modifierGroups);

  List<MenuModifier> GetModifiersByModifierGroupId(int modifierGroupId);

  public List<MenuModifierGroup> GetModifierGroupsByModifierId(int modifierId);

  public void RemoveCombinedModifierGroup(int modifierId, int groupId);

  public MenuItem GetItemById(int itemid);

  public string GetUnitById(int unitId);

  void AddModifierGroup(MenuModifierGroup modifierGroup);

  void UpdateModifierGroup(MenuModifierGroupVM modifierGroup);

  void UpdateModifier(MenuModifier modifier);

  void DeleteModifierGroup(MenuModifierGroup modifierGroup);

  void UpdateMenuItemModifierGroups(int itemId, List<ItemModifierGroup> modifierGroups);

  public void AddModifier(MenuModifier modifier);
  void DeleteModifiers(List<MenuModifier> modifiers);

  MenuCategoryVM GetEditMenuItemViewModel(int itemId);
  
  ServiceResult AddMenuItem(JsonObject menuItemData);

  ServiceResult UpdateMenuItemFromJson(JsonObject menuItemData, string imageFolderPath);

   int? AddModifierGroupWithModifiers(MenuModifierGroupVM model);

   bool UpdateModifierGroupWithModifiers(MenuModifierGroupVM model);

}