using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_core.DTO.UserSetting.Response
{
    public class UserSettingResponse
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsDarkMode { get; set; }
        public bool ReceiveNotifications { get; set; }
    }
}
