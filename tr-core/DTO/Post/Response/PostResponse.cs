using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.UserPlatform.Response;
using tr_core.Enums;

namespace tr_core.DTO.Post.Response
{
    public class PostResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string PromptText { get; set; }
        public string UserId { get; set; }
        public string Body { get; set; }
        public PostStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
