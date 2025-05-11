using System.ComponentModel.DataAnnotations;

namespace Entities.ViewModel
{
    public class PaginatedViewModel<T>
    {
        public T Data { get; set; }  // Holds any type of ViewModel
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int TotalItemCount { get; set; }

        public int PageCount => (int)Math.Ceiling((double)TotalItemCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < PageCount;
    }
}