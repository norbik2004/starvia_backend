namespace StarviaBackend.Modules.Media.Infrastructure.Storage.RustFs;

/// <summary>Connection settings for RustFS (S3-compatible object storage).</summary>
public sealed class RustFsOptions
{
    public const string SectionName = "rustfs";

    /// <summary>Base url of the S3 API, e.g. http://localhost:9000.</summary>
    public string Endpoint { get; set; } = "http://localhost:9000";

    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";

    /// <summary>RustFS (like MinIO) is addressed by path (host/bucket/key), not by virtual host.</summary>
    public bool ForcePathStyle { get; set; } = true;
}
