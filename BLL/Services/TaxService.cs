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

public class TaxService : ITaxService
{
    private readonly ITaxRepository _taxRepository;

    // private readonly IUserService _userService;

    public TaxService(ITaxRepository taxRepository)
    {
        _taxRepository = taxRepository;
    }

    public List<Taxis> GetAllTaxes()
    {
        return _taxRepository.GetAllTaxes();
    }

    public List<Taxis> GetIsEnabledAndIsDefaultTaxes()
    {
        return _taxRepository.GetIsEnabledAndIsDefaultTaxes();
    }

    public List<Taxis> GetIsEnabledAndNotIsDefaultTaxes()
    {
        return _taxRepository.GetIsEnabledAndNotIsDefaultTaxes();
    }

    public List<TaxType> GetAllTaxTypes()
    {
        return _taxRepository.GetAllTaxTypes();
    }

    public Taxis GetTaxById(int taxId)
    {
        return _taxRepository.GetTaxById(taxId);
    }
    public void AddTax(Taxis tax)
    {
        _taxRepository.AddTax(tax);
    }
    public bool UpdateTax(Taxis tax)
    {
        return _taxRepository.UpdateTax(tax);
    }
    public void DeleteTax(Taxis tax)
    {
        _taxRepository.DeleteTax(tax);
    }

    public IPagedList<TaxVM> getFilteredTaxes(UserFilterOptions filterOptions)
    {
        var taxes = _taxRepository.GetAllTaxes().AsQueryable();

        if (!string.IsNullOrEmpty(filterOptions.Search))
        {
            string searchLower = filterOptions.Search.ToLower();
            taxes = taxes.Where(u => u.TaxName.ToLower().Contains(searchLower) ||
                                     u.TaxType.TaxTypeName.ToLower().Contains(searchLower));
        }

        // Get total count and handle page size dynamically
        int totalTables = taxes.Count();
        int pageSize = filterOptions.PageSize > 0 ? Math.Min(filterOptions.PageSize, totalTables) : 10; // Default 10

        var paginatedtables = taxes
           .Select(tax => new TaxVM
           {
               TaxId = tax.TaxId,
               TaxName = tax.TaxName,
               IsEnabled = (bool)tax.IsEnabled,
               IsDefault = (bool)tax.IsDefault,
               IsDeleted = tax.IsDeleted,
               TaxTypeId = tax.TaxTypeId,
               TaxAmount = tax.TaxAmount,
               TaxTypeName = tax.TaxTypeId.HasValue ? _taxRepository.GetTaxTypeNameById(tax.TaxTypeId.Value) : "No Tax Type"
               // UnitName =  item.UnitId.HasValue ? _menuService.GetUnitById(item.UnitId.Value) : "No Unit"
           }).ToPagedList(filterOptions.Page.Value, filterOptions.PageSize);


        return paginatedtables;

    }

}
