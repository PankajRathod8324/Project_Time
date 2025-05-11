using Entities.Models;

namespace Entities.ViewModel
{
    public class CustomerTableVM
    {
        public List<Section> AllSections { get; set; }

        public List<Table> Tab { get; set; }
        public int SectionId { get; set; }

        public string SectionName { get; set; } = null!;

        public string SectionDecription { get; set; } = null!;

        public int TableId { get; set; }

        public string TableName { get; set; } = null!;
        public int Capacity { get; set; }
        public int? StatusId { get; set; }

        public string StatusName {get; set;}

        public bool? IsDeleted { get; set; }

        public int WaitingList { get; set; }

         public int CustomerTableId { get; set; }

    public int? CustomerId { get; set; }

    }
}