namespace RaceDay.Web.Services;

// What every API call gives back, so controllers never deal with exceptions
public class ApiResult
{
    public bool Success { get; set; }

    // 0 means the API could not be reached
    public int StatusCode { get; set; }

    public string Message { get; set; } = "";
}

public class ApiResult<T> : ApiResult
{
    public T? Data { get; set; }
}
