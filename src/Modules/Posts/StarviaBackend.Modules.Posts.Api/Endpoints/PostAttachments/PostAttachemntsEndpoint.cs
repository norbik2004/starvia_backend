using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.PostAttachments;

internal static class PostAttachmentsEndpoint
{
    public const string BasePath = "v1/post-attachments";
    public const string Tag = "PostAttachments";

    public const string AdminRole = UserRoles.Admin;
}
