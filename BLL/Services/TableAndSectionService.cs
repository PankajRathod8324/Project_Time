using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using  DAL.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Entities.ViewModel;
using X.PagedList.Extensions;
using X.PagedList;
using BLL.Interfaces;

namespace BLL.Services;

public class TableAndSectionService : ITableAndSectionService
{
    private readonly ITableAndSectionRepository _tableandsectionRepository;

    // private readonly IUserService _userService;

    public TableAndSectionService(ITableAndSectionRepository tableandsectionRepository)
    {
        _tableandsectionRepository = tableandsectionRepository;
    }

    public List<Section> GetAllSection()
    {
        var section = _tableandsectionRepository.GetAllSection();
        return section;
    }

    public Section GetSectionById(int sectionId)
    {
        return _tableandsectionRepository.GetSectionById(sectionId);
    }

    public string GetSectionNameByTableId(int tableId)
    {
        return _tableandsectionRepository.GetSectionNameByTableId(tableId);
    }

    public string GetTableNameByTableId(int tableId)
    {
        return _tableandsectionRepository.GetTableNameByTableId(tableId);
    }

    public void AddSection(SectionVM model)
    {
        var section = new Section
        {
            SectionName = model.SectionName,
            SectionDecription = model.SectionDecription
        };

        _tableandsectionRepository.AddSection(section);
    }

    public void EditSection(SectionVM model)
    {
        var section = _tableandsectionRepository.GetSectionById(model.SectionId);
        section.SectionName = model.SectionName;
        section.SectionDecription = model.SectionDecription;
        _tableandsectionRepository.EditSection(section);
    }

    public void DeleteSection(int sectionId)
    {
        var section = _tableandsectionRepository.GetSectionById(sectionId);
        _tableandsectionRepository.DeleteSection(section);

    }

    public IEnumerable<TableStatus> GetAllStatus()
    {
        return _tableandsectionRepository.GetAllStatus();
    }

    public List<Table> GetTablesBySectionId(int sectionId)
    {
        return _tableandsectionRepository.GetTablesBySectionId(sectionId);
    }

    public List<Table> GetAvailableTablesBySectionId(int sectionId)
    {
        return _tableandsectionRepository.GetAvailableTablesBySectionId(sectionId);
    }

    public string GetStatusById(int statusId)
    {
        return _tableandsectionRepository.GetStatusById(statusId);
    }

    public Table GetTableById(int tableId)
    {
        return _tableandsectionRepository.GetTableById(tableId);
    }

    public void AddTable(Table table)
    {
        _tableandsectionRepository.AddTable(table);
    }

    public bool UpdateTable(Table table)
    {
        return _tableandsectionRepository.UpdateTable(table);
    }

    public void DeleteTables(List<Table> tables)
    {
        _tableandsectionRepository.DeleteTables(tables);
    }

    public IPagedList<SectionVM> getFilteredMenuTables(int sectionId, UserFilterOptions filterOptions)
    {
        var tables = _tableandsectionRepository.GetTablesBySectionId(sectionId).AsQueryable();

        if (!string.IsNullOrEmpty(filterOptions.Search))
        {
            string searchLower = filterOptions.Search.ToLower();
            tables = tables.Where(u => u.TableName.ToLower().Contains(searchLower) ||
                                     u.Status.StatusName.ToLower().Contains(searchLower));
        }

        // Get total count and handle page size dynamically
        int totalTables = tables.Count();
        int pageSize = filterOptions.PageSize > 0 ? Math.Min(filterOptions.PageSize, totalTables) : 10; // Default 10

        var paginatedtables = tables
           .Select(table => new SectionVM
           {
               SectionId = (int)table.SectionId,
               TableId = table.TableId,
               TableName = table.TableName,
               Capacity = table.Capacity,
               StatusId = table.StatusId,
               IsDeleted = table.IsDeleted,
               StatusName = table.StatusId.HasValue ? _tableandsectionRepository.GetStatusById(table.StatusId.Value) : "No Status"
               // UnitName =  item.UnitId.HasValue ? _menuService.GetUnitById(item.UnitId.Value) : "No Unit"
           }).ToPagedList(filterOptions.Page.Value, filterOptions.PageSize);


        return paginatedtables;

    }
}