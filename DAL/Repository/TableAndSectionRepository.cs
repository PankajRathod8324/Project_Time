using DAL.Interfaces;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class TableAndSectionRepository : ITableAndSectionRepository
{
    private readonly PizzaShopContext _context;

    public TableAndSectionRepository(PizzaShopContext context)
    {
        _context = context;
    }


    public List<Section> GetAllSection()
    {
        return _context.Sections.Where(c => (bool)!c.IsDeleted).ToList();
    }


    public Section GetSectionById(int sectionId)
    {
        return _context.Sections.Find(sectionId);
    }

    public string GetSectionByTableId(int tableId)
    {
        var sectionName = (from tab in _context.Tables
                           join sec in _context.Sections on tab.SectionId equals sec.SectionId
                           where tab.TableId == tableId
                           select sec.SectionName).FirstOrDefault();
        return sectionName;
    }

    public void AddSection(Section section)
    {
        _context.Sections.Add(section);
        Save();
    }

    public void EditSection(Section section)
    {
        _context.Sections.Update(section);
        Save();
    }
    public void DeleteSection(Section section)
    {
        Console.WriteLine(section);
        if (section != null)
        {
            section.IsDeleted = true;
            Save();
        }
    }

    public string GetSectionNameByTableId(int tableId)
    {
        var sectionName = (from table in _context.Tables
                           join section in _context.Sections on table.SectionId equals section.SectionId
                           where table.TableId == tableId
                           select section.SectionName).FirstOrDefault();
        return sectionName;
    }

    public string GetTableNameByTableId(int tableId)
    {
        var tableName = (from table in _context.Tables
                         where table.TableId == tableId
                         select table.TableName).FirstOrDefault();
        return tableName;
    }

    public List<Table> GetTablesBySectionId(int sectionId)
    {
        if (sectionId == null || sectionId == 0)
        {
            sectionId = 3;
        }
        return _context.Tables.Where(i => i.SectionId == sectionId && (bool)!i.IsDeleted).ToList();
    }

    //  //The Tables dropdown shall display only the tables that are currently available (not occupied or already reserved).

    public List<Table> GetAvailableTablesBySectionId(int sectionId)
    {
        if (sectionId == null || sectionId == 0)
        {
            sectionId = 3;
        }
        return _context.Tables.Where(i => i.SectionId == sectionId && i.StatusId == 1 && (bool)!i.IsDeleted).ToList();
    }

    public Table GetTableById(int tableId)
    {
        return _context.Tables.FirstOrDefault(r => r.TableId == tableId);
    }


    public IEnumerable<TableStatus> GetAllStatus()
    {
        return _context.TableStatuses.ToList();
    }

    //  public string GetStatusById(int statusId)
    // {
    //     var grpName = (from modgrp in _context.MenuModifierGroups
    //                    where modgrp.ModifierGroupId == modifiergroupid
    //                    select modgrp.ModifierGroupName).FirstOrDefault();
    //     return grpName;

    // }

    public string GetStatusById(int statusId)
    {
        var tablestatus = (from table in _context.TableStatuses
                           where table.StatusId == statusId
                           select table.StatusName).FirstOrDefault();
        return tablestatus;
    }

    public void AddTable(Table table)
    {
        _context.Tables.Add(table);
        _context.SaveChanges();

    }

    public bool UpdateTable(Table table)
    {
        var existingItem = _context.Tables.FirstOrDefault(m => m.TableId == table.TableId);
        if (existingItem == null)
        {
            return false;
        }
        if (table.SectionId != null)
        {
            existingItem.SectionId = table.SectionId;
        }
        if (table.TableName != null)
        {
            existingItem.TableName = table.TableName;
        }
        if (table.Capacity != 0)
        {
            existingItem.Capacity = table.Capacity;
        }
        if (table.StatusId != null)
        {
            existingItem.StatusId = table.StatusId;
        }
        if (table.OccupiedTime != null)
        {
            existingItem.OccupiedTime = table.OccupiedTime;
        }



        Save();
        return true;
    }

    public void DeleteTables(List<Table> tables)
    {
        Console.WriteLine(tables);
        foreach (var p in tables)
        {
            Console.WriteLine($"itemsId:{p.TableId}");
        }

        // var item = _context.MenuItems.Where(i => items.Contains(i.ItemId)).ToList();

        foreach (var pr in tables)
        {
            var existingtables = _context.Tables.FirstOrDefault(p => p.TableId == pr.TableId);
            existingtables.IsDeleted = true;
        }
        Save();
    }

    public void Save()
    {
        _context.SaveChanges();
    }

}