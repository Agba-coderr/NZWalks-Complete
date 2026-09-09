namespace NZWalks.Application.Common
{
    public static class PaginationValidator
    {
        public static Result Validate(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                return Result.Failure("Page number must be greater than 0.", 400);
            }

            if (pageSize < 1)
            {
                return Result.Failure("Page size must be greater than 0.", 400);
            }

            if (pageSize > 50)
            {
                return Result.Failure("Page size cannot exceed 50.", 400);
            }

            return Result.Success(null, "Pagination parameters are valid.");
        }
    }
}
