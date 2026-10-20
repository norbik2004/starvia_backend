using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Posts.Integrations;

/// <summary>Wraps posts API failures in a domain exception the error middleware understands.</summary>
public sealed class PostsIntegrationException(string message) : BusinessException(message)
{
    public override string Code => "posts_integration_failed";
}

public sealed class PostsIntegrationNotOwnerException(string message) : BusinessException(message)
{
    public override string Code => "post_not_owned";
}
