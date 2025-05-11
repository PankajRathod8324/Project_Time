using System.ComponentModel.DataAnnotations;
using Entities.Models;

namespace Entities.ViewModel
{
    public class DashboardVM
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public decimal AvgOrderValue { get; set; }
        public decimal AvgWaitingTime { get; set; }
        public List<MenuCategoryVM> TopSellingItems { get; set; }
        public List<MenuCategoryVM> LeastSellingItems { get; set; }
        public int NewCustomers { get; set; }
        public int WaitingListCount { get; set; }
    }

}