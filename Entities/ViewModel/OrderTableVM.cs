using System.ComponentModel.DataAnnotations;
using Entities.Models;
using Npgsql.Internal.TypeHandlers.DateTimeHandlers;
using X.PagedList;

namespace Entities.ViewModel
{
    public class OrderTableVM
    {
        public int TableId { get; set; }

        public int SectionId { get; set; }

        public string SectionName { get; set; }

        public string TableName { get; set; }

        public int Capacity { get; set; }

        public string Status { get; set; }

        public TimeSpan OccuipiedTime { get; set; }

        public int? OrderId { get; set; }

        public int? CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }

        public string CustomerPhone { get; set; }

        public int NoOfPerson { get; set; }

        public DateOnly Date { get; set; }

        public int? ReviewId { get; set; }

        public int ReviewRating { get; set; }

        public string? Comment { get; set; }

        public decimal SubTotal { get; set; }

        public decimal TotalAmount { get; set; }

        public int? OrderStatusId { get; set; }

        public string OrderStatus { get; set; }

        public int? PaymentModeId { get; set; }

        public string PaymentMode { get; set; }

        public decimal OrderAmount { get; set; }



        public string TableaStatus { get; set; }

        public List<OrderTableVM> OrderTable { get; set; }

        public List<CustomerTable> CustomerTables {get; set;}

        public List<OrderVM> Order { get; set; }


    }
}