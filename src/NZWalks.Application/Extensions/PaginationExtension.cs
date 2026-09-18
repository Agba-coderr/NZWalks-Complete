namespace NZWalks.Application.Extensions
{
    public static class PaginationExtension
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            var skipResults = (pageNumber - 1) * pageSize;

            return query.Skip(skipResults).Take(pageSize);
        }
    }
}
