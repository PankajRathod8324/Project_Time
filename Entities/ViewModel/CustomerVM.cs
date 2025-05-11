using System.ComponentModel.DataAnnotations;
using Entities.Models;
using X.PagedList;

namespace Entities.ViewModel
{
    public class CustomerVM
    {
        public List<Customer> AllCustomers { get; set; }

        public IPagedList<Customer>? Customers { get; set; }

        public List<OrderVM> Orders {get; set;}

        public int CustomerId { get; set; }

        public string Name { get; set; } = null!;

        public int? TableId { get; set; }

        public string Email { get; set; } = null!;

        public string Phone { get; set; } = null!;
        
        public int NoOfPerson { get; set; }

        public int TotalOrder { get; set; }

        public decimal MaxOrder {get; set;}

        public decimal AverageOrder {get; set;}

        public int Visits {get; set;}

        public DateTime LastVisit {get; set;}

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ModifiedBy { get; set; }

        public DateTime ModifiedAt { get; set; }

        public DateOnly? Date { get; set; }



        public int? PaymentModeId { get; set; }

        public string PaymentMode { get; set; }
    }
}