namespace CpmServer.Common;

public class ApiResult
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }

    public static ApiResult Success(object? data = null)
        => new() { Code = 200, Message = "success", Data = data };

    public static ApiResult Error(string message)
        => new() { Code = 400, Message = message };
}

public class ApiResult<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResult<T> Success(T? data)
        => new() { Code = 200, Message = "success", Data = data };

    public static ApiResult<T> Error(string message)
        => new() { Code = 400, Message = message };
}
