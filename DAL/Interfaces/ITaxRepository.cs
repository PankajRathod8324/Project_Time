using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModel;

namespace  DAL.Interfaces;
public interface ITaxRepository
{
    List<Taxis> GetAllTaxes();
    List<TaxType> GetAllTaxTypes();

    List<Taxis> GetIsEnabledAndIsDefaultTaxes();

    List<Taxis> GetIsEnabledAndNotIsDefaultTaxes();
    

    string GetTaxNameById(int taxId);
    decimal GetTaxRateById(int taxId);

    int GetTexTypeIdByTaxId(int taxId);

    string GetTaxTypeNameById(int statusId);

    Taxis GetTaxById(int taxId);
    void AddTax(Taxis tax);
    bool UpdateTax(Taxis tax);
    void DeleteTax(Taxis tax);

}