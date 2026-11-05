using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Ai.Application.PostGeneration;

internal sealed class InvalidGeneratedPostException()
    : BusinessException("The AI model did not return a usable post. Try again.")
{
    public override string Code => "generated_post_invalid";
}
