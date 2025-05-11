using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModel;

namespace  DAL.Interfaces;

public interface IDashboardRepository
{
    DashboardVM GetAllDataFromDropdown(DateTime startDate, DateTime endDate);

    int GetSalesByHour(DateTime startDate, DateTime endDate, int hour);
    int GetSalesByDays(DateTime startDate);
    int GetTotalSalesByYears(DateTime startDate, DateTime endDate, int years);

    int GetTotalSalesByMonths(DateTime startDate, DateTime endDate, int months);

    int GetTotalCustomersByHours(DateTime startDate, DateTime endDate, int hour);

    int GetTotalCustomersByDays(DateTime startDate);

    int GetTotalCustomersByMonths(DateTime startDate, DateTime endDate, int months);

    int GetTotalCustomersByYears(DateTime startDate, DateTime endDate, int years);
}