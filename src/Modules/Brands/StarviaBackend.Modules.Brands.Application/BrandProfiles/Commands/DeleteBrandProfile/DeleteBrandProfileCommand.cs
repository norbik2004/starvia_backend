using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.DeleteBrandProfile;

public sealed record DeleteBrandProfileCommand(Guid BrandProfileId) : ICommand;
