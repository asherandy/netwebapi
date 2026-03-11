namespace BasicInformationOfDataWEBAPI.Common.DTOs
{
    /// <summary>
    /// 登录请求 DTO
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = null!;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = null!;


        /// <summary>
        /// uuid
        /// </summary>
        public string Uuid { get; set; } = null!;

        /// <summary>
        /// 验证
        /// </summary>
        public string Code { get; set; } = null!;



    }
}