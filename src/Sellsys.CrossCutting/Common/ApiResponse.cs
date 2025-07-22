using System.Net;

namespace Sellsys.CrossCutting.Common
{
    public class ApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;

        public static ApiResponse Success(string message = "Operation successful.")
        {
            return new ApiResponse { IsSuccess = true, Message = message, StatusCode = HttpStatusCode.OK };
        }

        public static ApiResponse Fail(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            return new ApiResponse { IsSuccess = false, Message = message, StatusCode = statusCode };
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }

        public static ApiResponse<T> Success(T data, string message = "Operation successful.")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message,
                StatusCode = HttpStatusCode.OK
            };
        }

        public static ApiResponse<T> Fail(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Data = default(T),
                Message = message,
                StatusCode = statusCode
            };
        }
    }
}