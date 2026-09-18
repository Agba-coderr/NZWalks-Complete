namespace NZWalks.Application.Common
{
    public class PagedResponse<T>
    {
        public IReadOnlyList<T> Data { get; init; } = [];
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
        public int TotalRecords { get; init; }
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;

        public static PagedResponse<T> Create(IReadOnlyList<T> data, int pageNumber, int pageSize, int totalRecords) 
        { 
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize); 

            return new PagedResponse<T> 
            { 
                Data = data, 
                PageNumber = pageNumber, 
                PageSize = pageSize, 
                TotalPages = totalPages, 
                TotalRecords = totalRecords 
            }; 
        }
    }
}
