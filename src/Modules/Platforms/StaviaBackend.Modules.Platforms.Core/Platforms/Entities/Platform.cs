using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using StarviaBackend.Modules.Platforms.Core.Platforms.Enums;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Platforms.Core.Platforms.Entities;

internal sealed class Platform : BaseEntity
{
    private readonly List<UserPlatform> _userPlatforms = [];

    public PlatformType PlatformType { get; private set; }
    public bool IsOnline { get; private set; }

    public IReadOnlyCollection<UserPlatform> UserPlatforms => _userPlatforms.AsReadOnly();

    private Platform()
    {

    }

    public static Platform Create(PlatformType platformType, bool IsOnline)
    {
        return new Platform
        {
            Id = Guid.NewGuid(),
            PlatformType = platformType,
            IsOnline = IsOnline
        };
    }

    public void UpdateStatus(bool status) => IsOnline = status;
}
