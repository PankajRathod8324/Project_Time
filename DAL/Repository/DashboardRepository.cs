using DAL.Interfaces;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class DashboardRepository : IDashboardRepository
{
    private readonly PizzaShopContext _context;

    public DashboardRepository(PizzaShopContext context)
    {
        _context = context;
    }

    public DashboardVM GetAllDataFromDropdown(DateTime startDate, DateTime endDate)
    {
        ///with timezone here get error
        startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);


        var orders = _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToList() ?? new List<Order>();
        

        var totalSales = orders.Sum(o => o.TotalAmount);
        var totalOrders = orders.Count;
        var avgOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;
        
        decimal avgwaitingtime;
        if(orders.Count != 0)
        {
            avgwaitingtime = (decimal)orders.Average(o => o.OrderDuration.HasValue ? o.OrderDuration.Value.TotalMinutes : 0);
        }
        else
        {
            avgwaitingtime = 0;
        }
       

        var waitinglist = _context.WaitingLists
            .Where(w => w.CreatedAt >= startDate && w.CreatedAt <= endDate)
            .ToList();

        var waitinglistcount = waitinglist.Count;

        var customers = _context.Customers
            .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
            .ToList() ?? new List<Customer>();

        var newcustomerlistcount = customers.Count;

        var orderitems = _context.OrderItems
            .Where(oi => oi.CreatedAt >= startDate && oi.CreatedAt <= endDate)
            .ToList() ?? new List<OrderItem>();

        var topsellingitmes = orderitems
            .GroupBy(oi => oi.ItemId)
            .Select(g => new
            {
                ItemId = g.Key,
                TotalQuantity = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(g => g.TotalQuantity)
            .Take(2)
            .ToList();

        var leastsellingitems = orderitems
            .GroupBy(oi => oi.ItemId)
            .Select(g => new
            {
                ItemId = g.Key,
                TotalQuantity = g.Sum(oi => oi.Quantity)
            })
            .OrderBy(g => g.TotalQuantity)
            .Take(2)
            .ToList();

        return new DashboardVM
        {
            TotalSales = totalSales,
            TotalOrders = totalOrders,
            AvgOrderValue = Math.Round(avgOrderValue, 2),
            NewCustomers = newcustomerlistcount,
            AvgWaitingTime = (decimal)Math.Round(avgwaitingtime, 2),
            WaitingListCount = waitinglistcount,
            TopSellingItems = topsellingitmes.Select(t => new MenuCategoryVM
            {
                ItemId = (int)t.ItemId,
                Quantity = t.TotalQuantity,
                ItemName = _context.MenuItems.FirstOrDefault(i => i.ItemId == t.ItemId)?.ItemName
            }).ToList(),
            LeastSellingItems = leastsellingitems.Select(t => new MenuCategoryVM
            {
                ItemId = (int)t.ItemId,
                Quantity = t.TotalQuantity,
                ItemName = _context.MenuItems.FirstOrDefault(i => i.ItemId == t.ItemId)?.ItemName
            }).ToList()
        };
    }

    public int GetSalesByHour(DateTime startDate, DateTime endDate, int hour)
    {
         startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        var salesCount = _context.Orders
            .Where(o => o.CreatedAt.Hour == hour && o.CreatedAt.Date == startDate.Date)
            .Sum(o => o.TotalAmount);

        return (int)salesCount;
    }

    public int GetSalesByDays(DateTime startDate)
    {
        startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

        // i want revenue not count
        var salesCount = _context.Orders
            .Where(o => o.CreatedAt.Date == startDate.Date)
            .Sum(o => o.TotalAmount);

        return (int)salesCount;
    }

    // Enumerable.Range(1, 12).Select(i => _dashboardRepository.GetTotalSalesByMonths(Start, End, i)).ToArray(),

    public int GetTotalSalesByMonths(DateTime startDate, DateTime endDate, int months)
    {
        startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        var salesCount = _context.Orders
            .Where(o => o.CreatedAt.Month == months && o.CreatedAt.Year == startDate.Year)
            .Sum(o => o.TotalAmount);

        return (int)salesCount;
    }

    public int GetTotalSalesByYears(DateTime startDate, DateTime endDate, int years)
    {
         startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        var salesCount = _context.Orders
            .Where(o => o.CreatedAt.Year == years)
            .Sum(o => o.TotalAmount);

        return (int)salesCount;
    }

    public int GetTotalCustomersByHours(DateTime startDate, DateTime endDate, int hour)
    {
         startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        var customerCount = _context.Customers
            .Where(c => c.CreatedAt.Hour == hour && c.CreatedAt.Date == startDate.Date)
            .Count();

        return customerCount;
    }

    public int GetTotalCustomersByDays(DateTime startDate)
    {
         startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

        var customerCount = _context.Customers
            .Where(c => c.CreatedAt.Date == startDate.Date)
            .Count();

        return customerCount;
    }

    public int GetTotalCustomersByMonths(DateTime startDate, DateTime endDate, int months)
    {
         startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        var customerCount = _context.Customers
            .Where(c => c.CreatedAt.Month == months && c.CreatedAt.Year == startDate.Year)
            .Count();

        return customerCount;
    }

    public int GetTotalCustomersByYears(DateTime startDate, DateTime endDate, int years)
    {
         startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        var customerCount = _context.Customers
            .Where(c => c.CreatedAt.Year == years)
            .Count();

        return customerCount;
    }


    public void Save()
    {
        _context.SaveChanges();
    }

}