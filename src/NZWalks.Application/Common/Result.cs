namespace NZWalks.Application.Common
{
    public class Result 
    {
        public bool IsSuccess { get; set; }

        public int Status { get; set; }

        public string Message { get; set; } = string.Empty;

        public dynamic? Data { get; set; }

        public static Result Success(dynamic? data, string message = "Success", int status = 200)
        {
            return new Result
            {
                IsSuccess = true,
                Status = status,
                Message = message,
                Data = data
            };
        }

        public static Result Failure(string message = "An error occurred", int status = 400)
        {
            return new Result
            {
                IsSuccess = false,
                Status = status,
                Message = message,
                Data = null
            };
        }
    }
}

