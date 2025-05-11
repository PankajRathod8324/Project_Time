using System.Diagnostics;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;

namespace PizzaShop.Controllers;


public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public IActionResult Index(string datefilter = "Today", DateTime? startDate = null, DateTime? endDate = null)
    {
        var dashboardData = _dashboardService.GetSalesData(datefilter, startDate, endDate);
        return PartialView("_DashboardPV",dashboardData);
    }

    public IActionResult LoadChart(string datefilter = "Today", DateTime? startDate = null, DateTime? endDate = null)
    {
        var chartData = _dashboardService.LoadChart(datefilter, startDate, endDate);
        return Json(chartData);
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}