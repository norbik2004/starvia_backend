using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace StarviaBackend.Shared.Infrastructure.Api;

/// <summary>
/// Lets modules keep their controllers / Ardalis endpoints <c>internal</c> while MVC still
/// discovers them. Mirrors the default provider but drops the public-visibility requirement.
/// </summary>
public sealed class InternalControllerFeatureProvider : ControllerFeatureProvider
{
    private const string ControllerSuffix = "Controller";

    protected override bool IsController(TypeInfo typeInfo)
    {
        if (!typeInfo.IsClass || typeInfo.IsAbstract || typeInfo.ContainsGenericParameters)
        {
            return false;
        }

        if (typeInfo.IsDefined(typeof(NonControllerAttribute)))
        {
            return false;
        }

        return typeInfo.Name.EndsWith(ControllerSuffix, StringComparison.OrdinalIgnoreCase)
               || typeInfo.IsDefined(typeof(ControllerAttribute))
               || IsAssignableToControllerBase(typeInfo);
    }

    private static bool IsAssignableToControllerBase(TypeInfo typeInfo)
    {
        var baseType = typeInfo.BaseType;
        while (baseType is not null)
        {
            if (baseType.Name.Contains("ControllerBase") || baseType.Name.Contains("EndpointBase"))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }
}
