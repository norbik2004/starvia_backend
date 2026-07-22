namespace StarviaBackend.Shared.Abstractions.Commands;

/// <summary>Marker for a command that does not return a result.</summary>
public interface ICommand;

/// <summary>Marker for a command that returns a result of <typeparamref name="TResult"/>.</summary>
public interface ICommand<out TResult>;
