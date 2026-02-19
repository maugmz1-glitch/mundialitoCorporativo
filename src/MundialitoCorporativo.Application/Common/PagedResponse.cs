namespace MundialitoCorporativo.Application.Common;

/// <summary>
/// REST response format for paginated data.
/// </summary>
public record PagedResponse<T>(IReadOnlyList<T> Data, int PageNumber, int PageSize, int TotalRecords, int TotalPages);
