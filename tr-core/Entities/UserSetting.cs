using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_core.Entities
{
    public class UserSetting : BaseEntity, IAuditable
    {
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public User User { get; set; } = null!;
        public bool IsDarkMode { get; set; }
        public bool ReceiveNotifications { get; set; }
    }
}
