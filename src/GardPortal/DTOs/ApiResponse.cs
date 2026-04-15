namespace GardPortal.DTOs;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public List<ApiError> Errors { get; set; } = new();

    public static ApiResponse<T> Success(T data) => new() { Data = data };

    public static ApiResponse<T> Failure(params ApiError[] errors)
        => new() { Errors = errors.ToList() };
}

public class ApiError
{
    public string Field   { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public ApiError() { }
    public ApiError(string field, string message) { Field = field; Message = message; }
}

public class PagedResponse<T> : ApiResponse<List<T>>
{
    public int TotalCount { get; set; }
    public int Page       { get; set; }
    public int PageSize   { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    public static PagedResponse<T> Success(List<T> data, int totalCount, int page, int pageSize)
        => new() { Data = data, TotalCount = totalCount, Page = page, PageSize = pageSize };
}
