namespace Entities.ViewModel 
{
    public class UserFilterOptions
    {
        public string Search { get; set; }
        public string FilterBy { get; set; }
        public bool? IsAsc { get; set; }
        public string SortBy { get; set; }
        public int? Page { get; set; }
        public int PageSize { get; set; } = 5; // Default value
    }
}