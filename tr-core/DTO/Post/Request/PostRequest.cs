using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.Enums;

namespace tr_core.DTO.Post.Request
{
    public class PostRequest
    {
        [MaxLength(75)]
        public string Title { get; set; } = null!;
        [MaxLength(500)]
        public string? Body { get; set; }
    }
}
