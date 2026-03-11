using System.ComponentModel.DataAnnotations;

namespace BasicInformationOfDataWEBAPI.Domain.Entities
{
    public class Simsuserinfo
    {
        [Key]
        public int Simsu_id { get; set; }

        // 登录用户名
        [Required]
        [MaxLength(50)]
        public string Simsu_name { get; set; } = null!;
        // 密码（存储哈希后的密码）
        [Required]
        [MaxLength(256)]
        public string Simsu_pwd { get; set; } = null!;

        // 邮箱（UserDto需要）
        [MaxLength(100)]
        public string? Simsu_Email { get; set; }

        // 用户状态（UserSessionDto需要）
        // 0: 正常, 1: 停用
        public int Simsu_state { get; set; } = 0;

        // 角色（UserSessionDto需要）
        public int Simsu_role { get; set; } = 0;

        // 权限类型（UserSessionDto需要）
        public int Simsu_PermissionType { get; set; } = 0;

        // 默认头像（UserSessionDto需要）
        [MaxLength(200)]
        public string? Simsu_Avatar { get; set; } = "default.png";

        // 可选：创建时间/更新时间（方便业务逻辑）
        public DateTime Simsu_time { get; set; } = DateTime.UtcNow;
    }


    //public class UserInfo
    //{
    //    public int UI_ID { get; set; }           // 主键
    //    public string UI_UserName { get; set; } = null!;
    //    public string UI_CName { get; set; } = null!;
    //    public string UI_Email { get; set; } = null!;
    //    // 导航属性：一个用户有多个订单
    //    public List<Order> Orders { get; set; } = new List<Order>();
    //}
    //public class Order
    //{
    //    public int OrderId { get; set; }         // 主键
    //    public decimal Amount { get; set; }

    //    // 外键字段
    //    public int UserId { get; set; }

    //    // 导航属性：指向 UserInfo
    //    [ForeignKey("UserId")]                   // 标注 UserId 是外键
    //    public UserInfo User { get; set; } = null!;
    //}
}
