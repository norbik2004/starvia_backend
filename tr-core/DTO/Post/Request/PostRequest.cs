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
        public required string Title { get; set; }
        [MaxLength(500)]
        public string? Body { get; set; }
    }
}
