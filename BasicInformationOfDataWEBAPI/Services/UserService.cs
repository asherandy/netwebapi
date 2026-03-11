using BasicInformationOfDataWEBAPI.Common.DTOs;
using BasicInformationOfDataWEBAPI.Infrastructure;
using BasicInformationOfDataWEBAPI.Redis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BasicInformationOfDataWEBAPI.Services
{

    public class UserService : IUserService
    {
        private readonly AppDbContext _context; // DbContext 实例，用于操作数据库

        private readonly IConfiguration _configuration;
        private readonly IRedisService _redisService;
        // 构造函数通过 DI 注入 DbContext
        public UserService(AppDbContext context, IConfiguration configuration, IRedisService redisService)
        {
            _context = context;               // 保存 DbContext 实例到私有字段
            _configuration = configuration;
            _redisService = redisService;
        }


        public async Task<List<UserDto>> GetUsersAsync(int uid, string? name)
        {
            // 构建查询对象（IQueryable<UserInfo>）
            var query = _context.Simsuserinfo.AsQueryable(); // IQueryable 可以动态拼接条件

            if (uid > 0 && !string.IsNullOrEmpty(name))
            {
                // 如果希望 UID 或用户名匹配（逻辑或），可以改成：
                query = query.Where(u => u.Simsu_id == uid || u.Simsu_name == name);
            }
            else if (uid > 0)
            {
                query = query.Where(u => u.Simsu_id == uid);
            }
            else if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(u => u.Simsu_name == name);
            }

            // 异步执行查询，将结果映射成 List<UserInfo>
            return await query.Select(u => new UserDto
            {
                Simsuid = u.Simsu_id,   // Entity 字段 → DTO 字段
                Simsuname = u.Simsu_name,             // 示例字段
                SimsuEmail = u.Simsu_Email ?? "",
                Simsustate = u.Simsu_state
            })
            .ToListAsync();
        }

        public async Task<UserSessionDto?> LoginAsync(string username, string password)
        {
            var userSessionDto = await _context.Simsuserinfo
               .Where(u =>
                   u.Simsu_name == username &&
                   u.Simsu_pwd == password)
               .Select(u => new UserSessionDto
               {
                   Simsuid = u.Simsu_id,
                   Simsuname = u.Simsu_name,
                   SimsuEmail = u.Simsu_Email ?? "",
                   Simsustate = u.Simsu_state,
                   Simsurole = u.Simsu_role,
                   SimsuPermissionType = u.Simsu_PermissionType,
                   SimsuAvatar = u.Simsu_Avatar ?? "default.png",
                   LoginTime = DateTime.Now
               })
               .FirstOrDefaultAsync(); // ✅ 只取一个


            // 2️⃣ 找不到用户则返回 null
            return userSessionDto;

        }
        /// <summary>
        /// 获取用户信息（带缓存策略）
        /// </summary>
        async Task<UserSessionDto?> IUserService.GetUserInfoAsync(int userId)
        {
            var cacheKey = $"user:info:{userId}";
            var lockKey = $"lock:user:{userId}";

            // 1️⃣ 先从 Redis 读取
            var cachedUser = await _redisService.GetAsync<UserSessionDto>(cacheKey);
            if (cachedUser != null)
                return cachedUser;

            // 2️⃣ 防击穿：加分布式锁
            var gotLock = await _redisService.LockAsync(lockKey, TimeSpan.FromSeconds(5));

            if (!gotLock)
            {
                // 等待 100ms 再读缓存（短暂轮询，防止大量请求直接打数据库）
                await Task.Delay(100);
                return await _redisService.GetAsync<UserSessionDto>(cacheKey);
            }

            try
            {
                var userSessionDto = await _context.Simsuserinfo
               .Where(u => u.Simsu_id == userId)
               .Select(u => new UserSessionDto
               {
                   Simsuid = u.Simsu_id,
                   Simsuname = u.Simsu_name,
                   SimsuEmail = u.Simsu_Email ?? "",
                   Simsustate = u.Simsu_state,
                   Simsurole = u.Simsu_role,
                   SimsuPermissionType = u.Simsu_PermissionType,
                   SimsuAvatar = u.Simsu_Avatar ?? "default.png",
                   LoginTime = DateTime.Now
               })
               .FirstOrDefaultAsync(); // ✅ 只取一个

                // 4️⃣ 防缓存穿透
                if (userSessionDto == null)
                {
                    await _redisService.SetAsync<UserSessionDto>(cacheKey, null, TimeSpan.FromMinutes(2));
                    return null;
                }

                // 5️⃣ 防雪崩：随机过期时间
                var randomSeconds = new Random().Next(60, 300);
                var expiration = TimeSpan.FromMinutes(15) + TimeSpan.FromSeconds(randomSeconds);

                await _redisService.SetAsync(cacheKey, userSessionDto, expiration);

                return userSessionDto;
            }
            finally
            {
                // 6️⃣ 解锁
                await _redisService.UnlockAsync(lockKey);
            }
        }
    }
}