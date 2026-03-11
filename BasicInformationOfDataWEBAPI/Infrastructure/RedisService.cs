using BasicInformationOfDataWEBAPI.Infrastructure; // 项目内部基础设施命名空间
using Microsoft.Extensions.Caching.Distributed;    // IDistributedCache 接口
using StackExchange.Redis;                         // Redis 核心库
using System.Text.Json;                            // JSON 序列化

namespace BasicInformationOfDataWEBAPI.Redis
{
    /// <summary>
    /// Redis 缓存操作封装
    /// 使用 IDistributedCache，并自动加前缀，支持序列化/反序列化
    /// </summary>
    public class RedisService : IRedisService
    {
        // 分布式缓存接口（底层由 AddStackExchangeRedisCache 实现）
        private readonly IDistributedCache _cache;

        // Redis Key 前缀（用于区分不同系统或环境）
        private readonly string _prefix;

        // 读取配置文件（用于读取默认过期时间等）
        private readonly IConfiguration _configuration;

        // 直接操作 Redis 数据库对象（用于分布式锁）
        private readonly IDatabase _database;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="cache">
        /// 通过 AddStackExchangeRedisCache 注册的 IDistributedCache
        /// </param>
        /// <param name="options">
        /// RedisCacheOptions（用于获取 InstanceName 作为前缀）
        /// </param>
        public RedisService(
            IDistributedCache cache,
            RedisCacheOptions options,
            IConfiguration configuration,
            IConnectionMultiplexer multiplexer)
        {
            // 判空保护
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            // 读取 InstanceName 作为 Key 前缀
            _prefix = options.InstanceName;

            _configuration = configuration;

            // 直接使用 StackExchange.Redis 数据库实例
            _database = multiplexer.GetDatabase(); 
        }

        /// <summary>
        /// 从 Redis 获取指定 Key 对应的值并反序列化
        /// </summary>
        public async Task<T?> GetAsync<T>(string key)
        {
            // 防止空 key
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Redis key 不能为空", nameof(key));

            // 自动拼接前缀
            var fullKey = $"{_prefix}{key}";

            // 从 Redis 读取字符串
            var json = await _cache.GetStringAsync(fullKey);

            // 如果不存在，返回默认值
            if (string.IsNullOrEmpty(json))
                return default;

            try
            {
                // 反序列化为泛型对象
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (JsonException)
            {
                // 如果 JSON 异常，避免程序崩溃
                return default;
            }
        }

        /// <summary>
        /// 将对象序列化后存入 Redis
        /// </summary>
        public async Task SetAsync<T>(string key, T? value, TimeSpan? expire = null)
        {
            // Key 不能为空
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Redis key 不能为空", nameof(key));

            // 值不能为空
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // 拼接完整 Key
            var fullKey = $"{_prefix}{key}";

            // 序列化对象为 JSON 字符串
            var json = JsonSerializer.Serialize(value);

            // 从配置读取默认过期时间（分钟）
            double? configuredMinutes =
                _configuration.GetValue<double?>("Redis:SessionExpireMinutes") ?? 0;

            // 三层优先级决定过期时间：
            TimeSpan expiration;

            if (expire.HasValue)
            {
                // 优先使用方法参数
                expiration = expire.Value;
            }
            else if (configuredMinutes > 0)
            {
                // 使用配置文件中的时间
                expiration = TimeSpan.FromMinutes((double)configuredMinutes);
            }
            else
            {
                // 默认 1 小时
                expiration = TimeSpan.FromHours(1);
            }

            // 设置缓存选项（绝对过期时间）
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            // 写入 Redis
            await _cache.SetStringAsync(fullKey, json, options);
        }

        /// <summary>
        /// 删除指定 Key
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Redis key 不能为空", nameof(key));

            var fullKey = $"{_prefix}{key}";

            // 删除 Redis Key
            await _cache.RemoveAsync(fullKey);
        }

        /// <summary>
        /// 分布式锁（简单版）
        /// 使用 SET key value NX EX 实现
        /// </summary>
        public async Task<bool> LockAsync(string key, TimeSpan expiry)
        {
            // 如果 Key 不存在才设置（NX）
            // 设置过期时间防止死锁
            return await _database.StringSetAsync(
                key,
                Guid.NewGuid().ToString(),
                expiry,
                When.NotExists);
        }

        /// <summary>
        /// 解锁（简单删除）
        /// ⚠ 存在误删风险（生产环境建议使用 Lua 校验 value）
        /// </summary>
        public async Task UnlockAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}