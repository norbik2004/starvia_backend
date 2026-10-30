using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Ai.Infrastructure.OpenRouter;

/// <summary>Wraps OpenRouter failures in a domain exception the error middleware understands.</summary>
internal sealed class OpenRouterIntegrationException(string message) : BusinessException(message)
{
    public override string Code => "openrouter_integration_failed";
}
