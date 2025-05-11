using  DAL.Interfaces;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace  DAL.Repository;

public class TaxRepository : ITaxRepository
{
    private readonly PizzaShopContext _context;

    public TaxRepository(PizzaShopContext context)
    {
        _context = context;
    }

    public List<Taxis> GetAllTaxes()
    {
        return _context.Taxes
            .Where(c => (bool)!c.IsDeleted)
            .OrderBy(c => c.TaxId).ToList();
    }

    public List<TaxType> GetAllTaxTypes()
    {
        return _context.TaxTypes.ToList();
    }

    public List<Taxis> GetIsEnabledAndIsDefaultTaxes()
    {
        return _context.Taxes
            .Where(c => (bool)!c.IsDeleted && (bool)c.IsEnabled && c.IsDefault)
            .OrderBy(c => c.TaxId).ToList();
    }

    public List<Taxis> GetIsEnabledAndNotIsDefaultTaxes()
    {
        return _context.Taxes
            .Where(c => (bool)!c.IsDeleted && (bool)c.IsEnabled && !c.IsDefault)
            .OrderBy(c => c.TaxId).ToList();
    }

    public string GetTaxTypeNameById(int statusId)
    {
        var taxtypename = (from tax in _context.TaxTypes
                           where tax.TaxTypeId == statusId
                           select tax.TaxTypeName).FirstOrDefault();
        return taxtypename;
    }

    public string GetTaxNameById(int taxId)
    {
        var taxName = (from tax in _context.Taxes
                        where tax.TaxId == taxId
                        select tax.TaxName).FirstOrDefault();
        return taxName;
    }

    public decimal GetTaxRateById(int taxId)
    {
        var taxRate = (from tax in _context.Taxes
                        where tax.TaxId == taxId
                        select tax.TaxAmount).FirstOrDefault();
        return taxRate;
    }

    public int GetTexTypeIdByTaxId(int taxId)
    {
        var taxTypeId= (from tax in _context.Taxes
                        where tax.TaxId == taxId
                        select tax.TaxTypeId).FirstOrDefault();
        return (int)taxTypeId;
    }

    public Taxis GetTaxById(int taxId)
    {
        return _context.Taxes.FirstOrDefault(r => r.TaxId == taxId);
    }

    public void AddTax(Taxis tax)
    {
        _context.Taxes.Add(tax);
        Save();

    }

    public bool UpdateTax(Taxis tax)
    {
        var existingTax = _context.Taxes.FirstOrDefault(m => m.TaxId == tax.TaxId);
        if (existingTax == null)
        {
            return false;
        }

        existingTax.TaxName = tax.TaxName;
        existingTax.TaxAmount = tax.TaxAmount;
        existingTax.TaxTypeId = tax.TaxTypeId;
        existingTax.IsEnabled = tax.IsEnabled;
        existingTax.IsDefault = tax.IsDefault;

        _context.SaveChanges();
        return true;
    }

    public void DeleteTax(Taxis tax)
    {
        Console.WriteLine(tax);
        if (tax != null)
        {
            tax.IsDeleted = true;
            Save();
        }
    }


    public void Save()
    {
        _context.SaveChanges();
    }

}