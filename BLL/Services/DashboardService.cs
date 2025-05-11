using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using DAL.Interfaces;
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
using Microsoft.AspNetCore.Mvc;

namespace BLL.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public DashboardVM GetSalesData(string filter, DateTime? startDate = null, DateTime? endDate = null)
    {
        var (Start, End) = GetDateRangeFromFilter(filter);
        if (filter == "Custome Date")
        {
            Start = startDate ?? DateTime.Today;
            End = endDate ?? DateTime.Today;
        }

        var dashboardData = _dashboardRepository.GetAllDataFromDropdown(Start, End);
        return dashboardData;
    }

    public JsonResult LoadChart(string datefilter = "Today", DateTime? startDate = null, DateTime? endDate = null)
    {
        var (Start, End) = GetDateRangeFromFilter(datefilter);
        TimeSpan diffrence = TimeSpan.Zero;
        if (datefilter == "Custome Date")
        {
            Start = startDate ?? DateTime.Today;
            End = endDate ?? DateTime.Today;
            diffrence = End - Start + TimeSpan.FromDays(1);
        }



        var chartData = new
        {
            revenuelbl = datefilter switch
            {
                "Today" => Enumerable.Range(0, 24).Select(i => $"{i}:00").ToArray(),
                "Last 7 Days" => Enumerable.Range(0, 7).Select(i => Start.AddDays(i).ToString("dd/MM/yyyy")).ToArray(),
                "Last 30 Days" => Enumerable.Range(0, 30).Select(i => Start.AddDays(i).ToString("dd/MM/yyyy")).ToArray(),
                "Current Month" => Enumerable.Range(1, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)).Select(i => new DateTime(DateTime.Today.Year, DateTime.Today.Month, i).ToString("dd/MM/yyyy")).ToArray(),
                "Current Year" => Enumerable.Range(1, DateTime.Today.Month).Select(i => new DateTime(DateTime.Today.Year, i, 1).ToString("MMMM yyyy")).ToArray(),
           
                "Custome Date" when diffrence.Days < 1 => Enumerable.Range(0, diffrence.Hours).Select(i => Start.AddHours(i).ToString("dd MMMM yyyy")).ToArray(),
                "Custome Date" when diffrence.Days < 30 => Enumerable.Range(0, diffrence.Days).Select(i => Start.AddDays(i).ToString("dd MMMM yyyy")).ToArray(),
                "Custome Date" when diffrence.Days < 60 => Enumerable.Range(0, diffrence.Days).Select(i => Start.AddDays(i).ToString("dd MMMM yyyy")).ToArray(),
                "Custome Date" when diffrence.Days < 365 => Enumerable.Range(0, (int)(diffrence.Days / 30 + 1)).Select(i => Start.AddMonths(i).ToString("MMMM yyyy")).ToArray(),
                "Custome Date" when diffrence.Days > 365 => Enumerable.Range(0, (int)(diffrence.Days / 365 + 1)).Select(i => Start.AddYears(i).ToString("MMMM yyyy")).ToArray(),
                "All Time" => Enumerable.Range(1, DateTime.UtcNow.Year).Select(i => new DateTime(DateTime.Today.Year, i, 1).ToString("MMMM yyyy")).ToArray(),
                _ => Array.Empty<string>()
            },
            revenuedt = new[]
            {
            new
                {
                    Label = "Sales",
                    Data = datefilter switch
                    {
                    "Today" => Enumerable.Range(0, 24).Select(i => _dashboardRepository.GetSalesByHour(Start, End, i)).Cast<object>().ToArray(),
                    "Last 7 Days" => Enumerable.Range(0, 7).Select(i => _dashboardRepository.GetSalesByDays(Start.AddDays(i))).Cast<object>().ToArray(),
                    "Last 30 Days" => Enumerable.Range(0, 30).Select(i => _dashboardRepository.GetSalesByDays(Start.AddDays(i))).Cast<object>().ToArray(),
                    "Current Month" => Enumerable.Range(1, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)).Select(i => _dashboardRepository.GetSalesByDays(new DateTime(DateTime.Today.Year, DateTime.Today.Month, i))).Cast<object>().ToArray(),
                    "Current Year" => Enumerable.Range(1, 12).Select(i => _dashboardRepository.GetTotalSalesByMonths(Start, End, i)).Cast<object>().ToArray(),
                    "Custome Date" when diffrence.Days < 1 => Enumerable.Range(0, diffrence.Hours).Select(i => _dashboardRepository.GetSalesByHour(Start, End, i)).Cast<object>().ToArray(),
                    "Custome Date" when diffrence.Days < 30 => Enumerable.Range(0, diffrence.Days).Select(i => _dashboardRepository.GetSalesByDays(Start.AddDays(i))).Cast<object>().ToArray(),
                    "Custome Date" when diffrence.Days < 60 => Enumerable.Range(0, diffrence.Days).Select(i => _dashboardRepository.GetSalesByDays(Start.AddDays(i))).Cast<object>().ToArray(),
                    "Custome Date" when diffrence.Days < 365 => Enumerable.Range(Start.Month, (int)diffrence.Days / 30 + 1).Select(i => _dashboardRepository.GetTotalSalesByMonths(Start, End, i)).Cast<object>().ToArray(),
                    "Custome Date" when diffrence.Days > 365 => Enumerable.Range(Start.Year, (int)diffrence.Days / 365 + 1).Select(i => _dashboardRepository.GetTotalSalesByYears(Start, End, i)).Cast<object>().ToArray(),
                    "All Time" => Enumerable.Range(1, 12).Select(i => _dashboardRepository.GetTotalSalesByMonths(Start, End, i)).Cast<object>().ToArray(),
                    _ => Array.Empty<object>()
                    },
                    BackgroundColor = "rgba(75, 192, 192, 0.2)",
                    BorderColor = "rgba(75, 192, 192, 1)",
                    BorderWidth = 1
                }
            },
            customerlbl = datefilter switch
            {
                "Today" => Enumerable.Range(0, 24).Select(i => $"{i}:00").ToArray(),
                "Last 7 Days" => Enumerable.Range(0, 7).Select(i => Start.AddDays(i).ToString("dd/MM/yyyy")).ToArray(),
                "Last 30 Days" => Enumerable.Range(0, 30).Select(i => Start.AddDays(i).ToString("dd/MM/yyyy")).ToArray(),
                "Current Month" => Enumerable.Range(1, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)).Select(i => new DateTime(DateTime.Today.Year, DateTime.Today.Month, i).ToString("dd/MM/yyyy")).ToArray(),
                "Current Year" => Enumerable.Range(1, DateTime.Today.Month).Select(i => new DateTime(DateTime.Today.Year, i, 1).ToString("MMMM yyyy")).ToArray(),
                "Custome Date" when diffrence.Days < 1 => Enumerable.Range(0, diffrence.Hours).Select(i => Start.AddHours(i).ToString("dd MMMM yyyy")).ToArray(),
                "Custome Date" when diffrence.Days < 30 => Enumerable.Range(0, diffrence.Days).Select(i => Start.AddDays(i).ToString("dd MMMM yyyy")).ToArray(),
                "Custome Date" when diffrence.Days < 365 => Enumerable.Range(0, (int)(diffrence.Days / 30 + 1)).Select(i => Start.AddMonths(i).ToString("MMMM yyyy")).ToArray(),
                "Custome Date" when diffrence.Days > 365 => Enumerable.Range(0, (int)(diffrence.Days / 365 + 1)).Select(i => Start.AddYears(i).ToString("MMMM yyyy")).ToArray(),
                "All Time" => Enumerable.Range(1, DateTime.UtcNow.Year).Select(i => new DateTime(DateTime.Today.Year, i, 1).ToString("MMMM yyyy")).ToArray(),
                _ => Array.Empty<string>()
            },
            customerdt = new[]
            {
            new
                {
                    Label = "Sales",
                    Data = datefilter switch
                    {
                    "Today" => Enumerable.Range(0, 24).Select(i => _dashboardRepository.GetTotalCustomersByHours(Start, End, i)).ToArray(),
                    "Last 7 Days" => Enumerable.Range(0, 7).Select(i => _dashboardRepository.GetTotalCustomersByDays(Start.AddDays(i))).ToArray(),
                    "Last 30 Days" => Enumerable.Range(0, 30).Select(i => _dashboardRepository.GetTotalCustomersByDays(Start.AddDays(i))).ToArray(),
                    "Current Month" => Enumerable.Range(1, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)).Select(i => _dashboardRepository.GetTotalCustomersByDays(new DateTime(DateTime.Today.Year, DateTime.Today.Month, i))).ToArray(),
                    "Current Year" => Enumerable.Range(1, 12).Select(i => _dashboardRepository.GetTotalCustomersByMonths(Start, End, i)).ToArray(),
                    "Custome Date" when diffrence.Days < 1 => Enumerable.Range(0, diffrence.Hours).Select(i => _dashboardRepository.GetTotalCustomersByHours(Start, End, i)).ToArray(),
                    "Custome Date" when diffrence.Days < 30 => Enumerable.Range(0, diffrence.Days).Select(i => _dashboardRepository.GetTotalCustomersByDays(Start.AddDays(i))).ToArray(),
                    "Custome Date" when diffrence.Days < 365 => Enumerable.Range(Start.Month, (int)(diffrence.Days / 30 + 1)).Select(i => _dashboardRepository.GetTotalCustomersByMonths(Start, End, i)).ToArray(),
                    "Custome Date" when diffrence.Days > 365 => Enumerable.Range(Start.Year, (int)(diffrence.Days / 365 + 1)).Select(i => _dashboardRepository.GetTotalCustomersByYears(Start, End, i)).ToArray(),
                    "All Time" => Enumerable.Range(1, 12).Select(i => _dashboardRepository.GetTotalCustomersByMonths(Start, End, i)).ToArray(),
                    _ => Array.Empty<int>()
                    },
                    BackgroundColor = "rgba(75, 192, 192, 0.2)",
                    BorderColor = "rgba(75, 192, 192, 1)",
                    BorderWidth = 1
                }
            },

        };
        return new JsonResult(chartData);
    }

    private (DateTime Start, DateTime End) GetDateRangeFromFilter(string filter)
    {
        DateTime today = DateTime.Today;
        DateTime now = DateTime.UtcNow;
        return filter switch
        {
            "Today" => (today, now),
            "Last 7 Days" => (today.AddDays(-6), now),
            "Last 30 Days" => (today.AddDays(-29), now),
            "Current Month" => (new DateTime(today.Year, today.Month, 1), now),
            "Current Year" => (new DateTime(today.Year, 1, 1), now),
            "All Time" => (DateTime.MinValue, now),
            _ => (today, today)
        };
    }
}
