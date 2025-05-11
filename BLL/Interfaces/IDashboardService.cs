using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace  BLL.Interfaces;

public interface IDashboardService
{
    DashboardVM GetSalesData(string filter, DateTime? startDate = null, DateTime? endDate = null);

    JsonResult LoadChart(string datefilter = "Today", DateTime? startDate = null, DateTime? endDate = null);
}