using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities
{
    public class ImagePromptFile : IAuditable
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(ImagePrompt))]
        public int ImagePromptId { get; set; }
        public ImagePrompt ImagePrompt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public required string FileName { get; set; }

        public required string FilePath { get; set; }
    }
}
