using System.ComponentModel.DataAnnotations;

namespace BasicInformationOfDataWEBAPI.Common.DTOs
{
    public class UserSessionDto
    {

        // 用户ID
        public int Simsuid { get; set; }
        // 用户名
        public string Simsuname { get; set; } = null!;
        // 用户邮箱
        public string SimsuEmail { get; set; } = null!;
        // 用户状态
        public int Simsustate { get; set; }


        // 角色
        public int Simsurole { get; set; }
        //
        public int SimsuPermissionType { get; set; }
        // 默认头像
        public string SimsuAvatar { get; set; } = "default.png";
        // SessionID
        public string SessionId { get; set; } = null!;

        public DateTime LoginTime { get; set; }
    }
}
