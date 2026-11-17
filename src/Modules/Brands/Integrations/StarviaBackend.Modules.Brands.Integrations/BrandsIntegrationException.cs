using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Brands.Integrations;

public sealed class BrandsIntegrationException(string message) : BusinessException(message)
{
    public override string Code => "brands_integration_error";
}

public sealed class BrandsIntegrationNotOwnerException(string message) : BusinessException(message)
{
    public override string Code => "brands_integration_not_owner";
}
