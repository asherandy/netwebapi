using BasicInformationOfDataWEBAPI.Common.DTOs;
using BasicInformationOfDataWEBAPI.Infrastructure;

namespace BasicInformationOfDataWEBAPI.Common.Helpers
{
    /// <summary>
    /// 单点登录中间件
    /// 用于校验当前请求的 JWT 是否仍然是“最新有效登录”
    /// 如果用户在其他设备重新登录，则旧 Token 自动失效
    /// </summary>
    public class SingleLoginMiddleware
    {
        // 下一个中间件委托
        // ASP.NET Core 中间件通过 RequestDelegate 串联形成管道
        private readonly RequestDelegate _next;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next">管道中的下一个中间件</param>
        public SingleLoginMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// 中间件核心逻辑
        /// 每次请求都会执行此方法
        /// </summary>
        /// <param name="context">当前 HTTP 请求上下文</param>
        /// <param name="redisService">Redis 服务（通过依赖注入获取）</param>
        public async Task Invoke(HttpContext context, IRedisService redisService)
        {
            // 判断当前请求是否已经通过 JWT 认证
            // 如果没有认证（例如访问登录接口），则不需要校验
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // 从 JWT Claim 中读取 userId
                // 这个值是在生成 JWT 时放进去的
                var userId = context.User.FindFirst("userId")?.Value;

                // 从 JWT Claim 中读取 sessionId
                // sessionId 用于判断是否是当前最新登录
                var sessionId = context.User.FindFirst("sessionId")?.Value;

                // 如果 userId 和 sessionId 都存在
                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(sessionId))
                {
                    // 从 Redis 中获取当前用户“最新的 sessionId”
                    // Redis 结构：
                    // login:user:{userId} = 当前有效 sessionId
                    var redisSessionId = await redisService.GetAsync<string>($"login:user:{userId}");

                    // 如果 Redis 中的 sessionId 和 JWT 中的不一致
                    // 说明用户已经在其他地方重新登录
                    // 当前这个 Token 已经过期（被踢下线）
                    if (redisSessionId != sessionId)
                    {
                        // 返回 401 未授权
                        context.Response.StatusCode = 401;

                        // 返回提示信息
                        await context.Response.WriteAsync("账号已在其他设备登录");

                        // 终止后续管道执行
                        return;
                    }
                    // 校验通过后刷新过期时间
                    await redisService.SetAsync($"login:user:{userId}", sessionId, TimeSpan.FromMinutes(30));

                    // 1️⃣ 先获取当前 session 的用户信息
                    var userInfo = await redisService.GetAsync<UserSessionDto>(
                        $"login:session:{sessionId}"
                    );

                    if (userInfo == null)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("登录状态已失效");
                        return;
                    }

                    await redisService.SetAsync($"login:session:{sessionId}", userInfo, TimeSpan.FromMinutes(30));
                }
            }

            // 如果校验通过，继续执行下一个中间件
            await _next(context);
        }
    }
}