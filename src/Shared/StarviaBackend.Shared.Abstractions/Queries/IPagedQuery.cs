namespace StarviaBackend.Shared.Abstractions.Queries;

/// <summary>A query that returns a <see cref="PagedResult{T}"/> and carries paging parameters.</summary>
public interface IPagedQuery<T> : IQuery<PagedResult<T>>
{
    int Page { get; }
    int PageSize { get; }
}
