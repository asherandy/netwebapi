using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJwtToken(string username, int userId, string sessionId, string role = "User", int Minutes = 30)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),

            // 🔹 新增：用户ID
            new Claim("userId", userId.ToString()),

            // 🔹 新增：SessionId（用于单点登录）
            new Claim("sessionId", sessionId),

            new Claim(ClaimTypes.Role, role),

            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var keyString = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(keyString))
            throw new Exception("Jwt:Key 未配置");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 构造函数已经注入 IConfiguration

        // 优先使用传入 Minutes
        // 配置里有值就用配置
        // 都没有就默认 30 分钟
        int configuredMinutes = _configuration.GetValue<int?>("Jwt:ExpireMinutes") ?? 0;


        int expireMinutes = Minutes > 0 ? Minutes : configuredMinutes > 0 ? configuredMinutes : 30;


        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
public interface IJwtService
{
    string GenerateJwtToken(string username, int userId, string sessionId, string role = "User", int Minutes = 30);
}
