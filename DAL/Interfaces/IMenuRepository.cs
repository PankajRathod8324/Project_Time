using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModel;

namespace  DAL.Interfaces;
public interface IMenuRepository
{

  IEnumerable<MenuCategory> GetAllCategories();

   List<MenuItem> GetAllItems();

  MenuCategory GetCategoryById(int id);

   int GetTotalModifierCount();

   bool IsItemFavourite(int itemId, int userId);

  void AddCategory(MenuCategory category);

  void UpdateCategory(MenuCategoryVM category);

  void DeleteCategory(int categoryId);
  List<MenuItem> GetItemsByCategoryId(int categoryId);
  void AddMenuItem(MenuItem menuItem);

  void AddMenuItemModifierGroup(ItemModifierGroup menuitemmodifier);


  public void DeleteItems(List<MenuItem> items);

  public IEnumerable<Itemtype> GetAllItemTypes();


  List<MenuModifier> GetModifiers();
  public IEnumerable<Unit> GetAllUnits();

  public IEnumerable<MenuModifierGroup> GetAllModifierGroups();


  MenuModifierGroup GetModifierGroupById(int id);

  public void AddCombinedModifierGroup(CombineModifier modifierGroup);

  public MenuModifier GetModifierById(int modifierid);

  //  string GetModifierNameById(int modifierId, MenuModifierGroupVM modifierGroups);

  public MenuItem GetItemById(int itemid);

  string GetItemNameById(int itemId);

  List<ItemModifierGroup> GetItemModifier(int itemid);

  public string GetUnitById(int unitId);

  public string GetModifierGroupNameById(int modifiergroupid);

  public string GetModifierNameById(int modifierId);

  public decimal GetModifierPriceById(int itemId);
   public int GetModifierQuantityById(int itemId);

  List<MenuModifier> GetModifiersByModifierGroupId(int modifierGroupId);

  public List<MenuModifierGroup> GetModifierGroupsByModifierId(int modifierId);

  public void RemoveCombinedModifierGroup(int modifierId, int groupId);
  void AddModifierGroup(MenuModifierGroup modifierGroup);

  void UpdateModifierGroup(MenuModifierGroupVM modifierGroup);

  void DeleteModifierGroup(MenuModifierGroup modifierGroup);

  public void AddModifier(MenuModifier modifier);

  void UpdateModifier(MenuModifier modifier);

  void DeleteModifiers(List<MenuModifier> modifiers);

  void DeleteModifierGroupsByItemId(int itemId);

  public bool UpdateMenuItem(MenuItem menuItem);



}