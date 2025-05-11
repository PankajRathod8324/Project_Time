using System.ComponentModel.DataAnnotations;
using Entities.Models;
using X.PagedList;

namespace Entities.ViewModel
{
    public class OrderSectionVM
    {
        public List<OrderTableVM> Table { get; set; }

        public int Available {get; set;}

        public int Assigned {get; set;}
        
        public int Running {get; set;}

        public int Reserved {get; set;}
        
        public int SectionId { get; set; }

        public string SectionName { get; set; } = null!;

        public string SectionDecription { get; set; } = null!;

        public int TableId { get; set; }

        public string TableName { get; set; } = null!;
        public int Capacity { get; set; }
        public int? StatusId { get; set; }

        public string StatusName {get; set;}

        public bool? IsDeleted { get; set; }


    }
}