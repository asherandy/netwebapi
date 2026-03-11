

using StackExchange.Redis;

namespace BasicInformationOfDataWEBAPI.Infrastructure
{
    /// <summary>
    /// Redis 服务接口，用于封装 Redis 的基本操作
    /// 适用于多实例 WebAPI，保证状态共享
    /// </summary>
    public interface IRedisService
    {
        /// <summary>
        /// 从 Redis 获取指定 Key 对应的值并反序列化为指定类型
        /// </summary>
        /// <typeparam name="T">返回对象类型</typeparam>
        /// <param name="key">Redis Key</param>
        /// <returns>
        /// 如果 Key 存在，则返回对应对象；否则返回默认值（null）
        /// </returns>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// 将对象序列化后存入 Redis
        /// </summary>
        /// <typeparam name="T">存储对象类型</typeparam>
        /// <param name="key">Redis Key</param>
        /// <param name="value">要存储的对象</param>
        /// <param name="expire">
        /// 可选参数，Redis Key 的过期时间
        /// 如果为 null，则 Key 永不过期
        /// </param>
        Task SetAsync<T>(string key, T? value, TimeSpan? expire = null);


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task RemoveAsync(string key);


        /// <summary>
        /// 防缓存击穿：加锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        Task<bool> LockAsync(string key, TimeSpan expiry);


        /// <summary>
        /// 解锁
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task UnlockAsync(string key);








    }


    


}