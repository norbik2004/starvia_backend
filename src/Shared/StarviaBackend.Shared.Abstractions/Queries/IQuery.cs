namespace StarviaBackend.Shared.Abstractions.Queries;

/// <summary>Marker for a query returning <typeparamref name="TResult"/>.</summary>
public interface IQuery<out TResult>;
