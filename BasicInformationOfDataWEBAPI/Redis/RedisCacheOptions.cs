namespace BasicInformationOfDataWEBAPI.Redis
{
    /// <summary>
    /// Redis 配置封装类，用于集中管理 Redis 配置信息
    /// </summary>
    public class RedisCacheOptions
    {
        /// <summary>
        /// Redis 服务器地址，例如 "127.0.0.1:6379"
        /// </summary>
        public string RedisHost { get; set; } = "";

        /// <summary>
        /// Redis 密码，可为空
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Redis Key 前缀，例如 "BasicInformationAPI:"，用于避免不同应用 key 冲突
        /// </summary>
        public string InstanceName { get; set; } = "";

        /// <summary>
        /// Redis 连接超时时间
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);
    }
}