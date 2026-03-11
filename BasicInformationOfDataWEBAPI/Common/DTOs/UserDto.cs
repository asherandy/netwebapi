namespace BasicInformationOfDataWEBAPI.Common.DTOs
{
    public class UserDto
    {
        // 用户ID
        public int Simsuid { get; set; }
        // 用户名
        public string Simsuname { get; set; } = null!;
        // 用户邮箱
        public string SimsuEmail { get; set; } = null!;
        // 用户状态
        public int Simsustate { get; set; }



    }
}
