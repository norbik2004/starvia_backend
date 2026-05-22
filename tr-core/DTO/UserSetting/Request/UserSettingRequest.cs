using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_core.DTO.UserSetting.Request
{
    public class UserSettingRequest
    {
        public bool IsDarkMode { get; set; }
        public bool ReceiveNotifications { get; set; }
    }
}
