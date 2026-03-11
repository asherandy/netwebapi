using BasicInformationOfDataWEBAPI.Common.DTOs;
using BasicInformationOfDataWEBAPI.Common.Helpers;
using BasicInformationOfDataWEBAPI.Infrastructure;
using BasicInformationOfDataWEBAPI.Services;
using Microsoft.AspNetCore.Mvc;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StackExchange.Redis;


namespace BasicInformationOfDataWEBAPI.Controllers
{
    public class LoginController
        (IConfiguration configuration, IUserService userService, IJwtService jwtService, IRedisService redisService
        ) : ControllerBase
    {
        // 注入配置服务，用于读取应用配置信息
        private readonly IConfiguration _configuration = configuration;
        private readonly IUserService _userService = userService;
        private readonly IJwtService _jwtService = jwtService;
        private readonly IRedisService _redisService = redisService;

        // 定义POST类型的/login端点，处理用户登录请求
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {


            var captchacode = await _redisService.GetAsync<string>("captcha:" + request.Uuid);
            if (request.Code != captchacode)
                return Ok(new { msg = "验证码错误", code = 500 });



            var password = UtilHelper.EncrypToSHA(request.Password.Trim());
            UserSessionDto? result = await _userService.LoginAsync(request.Username, password);

            if (result == null)
                return Ok(new { msg = "用户名或密码错误", code = 500 });

            var sessionId = Guid.NewGuid().ToString(); // 使用GUID生成会话ID
            result.SessionId = sessionId;

            var redisKey = $"login:user:{result.Simsuid}";
            var sessionKey = $"login:session:{result.SessionId}";
            // 1️⃣ 查询旧 session
            var oldSessionId = await _redisService.GetAsync<string>(redisKey);
            // 返回对象（登录信息）
            //var userInfo222 = await _redisService.GetAsync<UserSessionDto>("login:session:51cde082-74c0-450f-a73d-26896b42ef81");

            if (!string.IsNullOrEmpty(oldSessionId))
            {
                // 删除旧 session 信息
                await _redisService.RemoveAsync($"login:session:{oldSessionId}");
            }


            // 存入 Redis（比如 30分钟）
            await _redisService.SetAsync(redisKey, sessionId, TimeSpan.FromMinutes(30));
            // 存入 redis  用户登录信息
            await _redisService.SetAsync(sessionKey, result, TimeSpan.FromMinutes(30));

            // 登录成功，生成JWT令牌
            var token = _jwtService.GenerateJwtToken(result.Simsuname, result.Simsuid, result.SessionId);


            // 返回200 OK状态码及令牌信息
            return Ok(new { Token = token });

        }

        [HttpGet("captchaImage")]
        public async Task<IActionResult> CaptchaImage()
        {
            var code = GenerateCode(4);
            var uuid = Guid.NewGuid().ToString();

            var imageBytes = GenerateImage(code);

            var base64 = Convert.ToBase64String(imageBytes);

            // TODO: 建议存入 Redis (uuid -> code)
            await _redisService.SetAsync("captcha:" + uuid, code, TimeSpan.FromMinutes(5));

            return Ok(new
            {
                captchaEnabled = true,
                img = base64,
                uuid = uuid
            });
        }

        private string GenerateCode(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private byte[] GenerateImage(string code)
        {
            using var image = new Image<Rgba32>(120, 40);

            image.Mutate(ctx =>
            {
                ctx.Fill(Color.White);

                var font = SystemFonts.CreateFont("Arial", 24);

                ctx.DrawText(code, font, Color.Black, new PointF(10, 5));
            });

            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return ms.ToArray();
        }



    }
}
