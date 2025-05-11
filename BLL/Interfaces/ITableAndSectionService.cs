using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.AspNetCore.Http;
using X.PagedList;

namespace  BLL.Interfaces;
public interface ITableAndSectionService
{
   List<Section> GetAllSection();

   string GetSectionNameByTableId(int tableId);

    Section GetSectionById(int sectionId);

   void AddSection(SectionVM model);

   void EditSection(SectionVM model);

   void DeleteSection(int sectionId);

   IEnumerable<TableStatus> GetAllStatus();

   List<Table> GetTablesBySectionId(int sectionId);

   string GetTableNameByTableId(int tableId);

   List<Table> GetAvailableTablesBySectionId(int sectionId);

   IPagedList<SectionVM> getFilteredMenuTables(int sectionId, UserFilterOptions filterOptions);

   string GetStatusById(int statusId);

    void AddTable(Table table);

    Table GetTableById(int tableId);

    bool UpdateTable(Table table);

     void DeleteTables(List<Table> tables);
}