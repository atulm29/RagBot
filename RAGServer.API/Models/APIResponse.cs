using System.Net;

namespace RAGSERVERAPI.Models;

public class APIResponse<T>
{
    public bool Success { get; set; }
    public bool IsValidationError { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string Message { get; set; }
    public T Result { get; set; }
    public object? Error { get; set; }
    public APIResponse(T data, string message = "", HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Success = true;
        IsValidationError = false;
        StatusCode = statusCode;
        Message = message;
        Result = data;
        Error = null;
    }
    public APIResponse(HttpStatusCode statusCode, string message,  object error = null,  bool isValidationError = false)
    {
        Success = false;
        IsValidationError = isValidationError;
        StatusCode = statusCode;
        Message = message;
        Result = default(T);
        Error = error;
    }
}
