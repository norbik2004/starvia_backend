using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;

namespace StarviaBackend.Modules.Posts.Core.Posts.Repositories;

internal interface IPostPublicationRepository
{
    Task AddAsync(PostPublication postPublication);
}
