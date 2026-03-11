using StackExchange.Redis;   // Redis 核心客户端
using System;                // 基础类型（TimeSpan 等）
using System.Threading.Tasks;// 异步支持

namespace BasicInformationOfDataWEBAPI.Redis
{
    /// <summary>
    /// 分布式锁封装
    /// 基于 Redis 的 SET NX + Lua 实现
    /// </summary>
    public class DistributedLock
    {
        // Redis 数据库实例（线程安全，可复用）
        private readonly IDatabase _db;

        /// <summary>
        /// 构造函数
        /// 通过 IConnectionMultiplexer 获取 IDatabase
        /// </summary>
        public DistributedLock(IConnectionMultiplexer redis)
        {
            // 获取默认数据库（通常是 DB 0）
            _db = redis.GetDatabase();
        }

        /// <summary>
        /// 尝试获取锁
        /// </summary>
        /// <param name="key">锁的 Key（例如 lock:user:1）</param>
        /// <param name="value">
        /// 锁的唯一值（必须是 Guid）
        /// 用于防止误删别人的锁
        /// </param>
        /// <param name="expiry">
        /// 锁的过期时间（防止死锁）
        /// </param>
        /// <returns>
        /// true = 获取成功
        /// false = 获取失败（已被占用）
        /// </returns>
        public async Task<bool> AcquireAsync(string key, string value, TimeSpan expiry)
        {
            // Redis 命令：
            // SET key value NX EX seconds
            //
            // NX = 仅当 key 不存在时才设置
            // EX = 设置过期时间
            //
            // 原子操作，不会并发冲突
            return await _db.StringSetAsync(
                key,
                value,
                expiry,
                When.NotExists);
        }

        /// <summary>
        /// 释放锁（安全版本）
        /// </summary>
        /// <param name="key">锁的 Key</param>
        /// <param name="value">
        /// 必须传入获取锁时的 value
        /// 用于校验锁是否属于当前线程
        /// </param>
        /// <returns>true = 删除成功</returns>
        public async Task<bool> ReleaseAsync(string key, string value)
        {
            // Lua 脚本保证原子性：
            //
            // 逻辑：
            // 1️⃣ 判断当前 key 的值是否等于传入 value
            // 2️⃣ 如果相等 → 删除
            // 3️⃣ 不相等 → 返回 0（说明锁已被别人占用）
            //
            // 这样可以防止：
            // A 获取锁
            // 锁过期
            // B 获取锁
            // A 删除锁 ❌（误删）
            //
            const string script = @"
if redis.call('get', KEYS[1]) == ARGV[1] then
    return redis.call('del', KEYS[1])
else
    return 0
end";

            // 执行 Lua 脚本
            var result = (int)await _db.ScriptEvaluateAsync(
                script,
                new RedisKey[] { key },     // KEYS[1]
                new RedisValue[] { value }  // ARGV[1]
            );

            // 如果返回 1 表示删除成功
            return result == 1;
        }
    }
}