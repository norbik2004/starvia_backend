namespace StarviaBackend.Shared.Abstractions.App;

/// <summary>Public base URLs used to build email links and post-action frontend redirects.</summary>
public interface IAppUrls
{
    string ApiBaseUrl { get; }
    string FrontendBaseUrl { get; }
}
