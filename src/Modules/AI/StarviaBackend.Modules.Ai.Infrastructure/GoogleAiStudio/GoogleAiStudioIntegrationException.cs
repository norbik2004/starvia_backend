using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Ai.Infrastructure.GoogleAiStudio;

/// <summary>Wraps Google AI Studio failures in a domain exception the error middleware understands.</summary>
internal sealed class GoogleAiStudioIntegrationException(string message) : BusinessException(message)
{
    public override string Code => "google_ai_studio_integration_failed";
}
