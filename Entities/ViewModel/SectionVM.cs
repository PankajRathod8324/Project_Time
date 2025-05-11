using System.ComponentModel.DataAnnotations;
using Entities.Models;
using X.PagedList;

namespace Entities.ViewModel
{
    public class SectionVM
    {
        public List<Section> AllSections { get; set; }

        public IPagedList<Table>? tables { get; set; }

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

    }
}