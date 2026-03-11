using BasicInformationOfDataWEBAPI.Common.DTOs;
using BasicInformationOfDataWEBAPI.Infrastructure;
using BasicInformationOfDataWEBAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BasicInformationOfDataWEBAPI.Controllers
{
    /// <summary>
    /// 默认
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        /// <summary>
        /// 随机数组
        /// </summary>
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IRedisService _redisService;
        private readonly IUserService _userService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration,
            IRedisService redisService, IUserService userService)
        {
            _logger = logger;
            _configuration = configuration;
            _redisService = redisService;
            _userService = userService;
        }


        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // 从 JWT Claims 获取用户ID
            var userIdClaim = User.FindFirst("userId")?.Value; 

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { Message = "用户未登录或身份无效" });

            // 调用 Service 获取用户信息（内部有 Redis + 锁 + 防雪崩处理）
            var user = await _userService.GetUserInfoAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(user);
        }


        /// <summary>
        /// 文件不能超过 5MB  ，只允许上传 jpg/png/pdf 文件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("Post")]
        public async Task<IActionResult> Post([FromForm] FileUploadRequest request)
        {
            // 验证上传的文件是否存在且不为空
            if (request.File == null || request.File.Length == 0)
                // 如果文件不存在或为空，返回400错误状态码和错误消息
                return BadRequest("No file uploaded.");

            // 构建文件保存路径，将文件保存到uploads目录下
            var filePath = Path.Combine("uploads", request.File.FileName);
            // 确保文件保存目录存在，如果不存在则创建目录
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            // 使用FileStream创建文件流，将上传的文件保存到指定路径
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                // 异步复制文件内容到文件流中
                await request.File.CopyToAsync(stream);
            }

            // 返回200 OK状态码，包含文件保存路径的成功响应
            return Ok(new { FilePath = filePath });
        }


    }

}
