using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Enums;

namespace Core.Entities
{
    public class Platform : BaseEntity, IAuditable
    {
        public PlatformType Type { get; set; }
        public ICollection<UserPlatform> UserPlatforms { get; set; } = [];
    }
}
