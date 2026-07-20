using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Consts;

namespace Core.Application.DTO.UserPrompt.Request
{
    public class UserPromptQueryParams : PaginatedListQueryParams
    {
        public DateTime? CreatedBefore { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public string? PromptContains { get; set; }
        public UserPromptSortBy SortBy { get; set; } = UserPromptSortBy.Id;
    }

    public enum UserPromptSortBy
    {
        Id,
        CreatedAt,
        UpdatedAt,
        Prompt
    }

}
