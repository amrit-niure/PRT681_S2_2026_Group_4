namespace WorkforceApi.Dtos;

/// <summary>One page of results plus the total number of matches, as the Kendo grid expects.</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public int Total { get; init; }
}
