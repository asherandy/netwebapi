// 引入 ASP.NET API 版本管理命名空间，用于 AddApiVersioning 和相关类型
using Asp.Versioning;
using BasicInformationOfDataWEBAPI.Common.Helpers;


// 引入自定义基础设施命名空间，一般包含 DbContext、仓储等
using BasicInformationOfDataWEBAPI.Infrastructure;
using BasicInformationOfDataWEBAPI.Redis;
using BasicInformationOfDataWEBAPI.Services;



// 引入 JWT Bearer 认证相关命名空间
using Microsoft.AspNetCore.Authentication.JwtBearer;

// 引入 Entity Framework Core 数据库上下文和操作
using Microsoft.EntityFrameworkCore;

// 引入 JWT 令牌验证参数类型
using Microsoft.IdentityModel.Tokens;

// 引入 Swagger/OpenAPI 定义类型
using Microsoft.OpenApi.Models;
using Prometheus;
using StackExchange.Redis;


// 引入程序集反射相关，用于获取 XML 注释文件
using System.Reflection;

// 引入字符串编码，用于生成 JWT 对称密钥
using System.Text;


// 创建 WebApplicationBuilder 对象，读取命令行参数 args 并初始化配置、服务容器
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 如果运行在 Windows，启用 Windows 服务模式
if (OperatingSystem.IsWindows())
{
    builder.Host.UseWindowsService();
}


// ======================= 添加核心服务 =======================

// 添加 MVC 控制器服务，使应用支持 API 控制器
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


// 请求统计中间件
builder.Services.AddSingleton<RequestTracker>();


// 开启端点 API 探索器（Swagger 使用），用于生成 OpenAPI 描述
builder.Services.AddEndpointsApiExplorer();

// 添加 Swagger 生成器服务，生成 Swagger 文档
builder.Services.AddSwaggerGen();


// ======================= JWT 身份验证 =======================
// 配置JWT身份验证服务
builder.Services.AddAuthentication(options =>
{
    // 设置默认认证方案为JWT Bearer认证
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    // 设置默认挑战方案为JWT Bearer认证
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    // 添加JWT Bearer认证处理器

    // 配置令牌验证参数
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // 验证签发者（Issuer）
        ValidateIssuer = true,
        // 验证受众（Audience）
        ValidateAudience = true,
        // 验证令牌有效期
        ValidateLifetime = true,
        // 验证签名密钥
        ValidateIssuerSigningKey = true,
        // 设置有效的签发者
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        // 从配置中读取有效的受众
        ValidAudience = builder.Configuration["Jwt:Audience"],
        // 从配置中读取用于验证签名的对称安全密钥
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// 添加授权服务，用于控制对API端点的访问权限
builder.Services.AddAuthorization();


// ======================= API 版本管理 =======================

// 添加API版本控制服务
builder.Services.AddApiVersioning(options =>
{
    // 设置默认版本为 1.0
    options.DefaultApiVersion = new ApiVersion(1, 0);
    // 当请求未指定版本时，默认使用上面设置的版本
    options.AssumeDefaultVersionWhenUnspecified = true;
    // 在响应头返回当前支持的 API 版本信息
    options.ReportApiVersions = true;
    // 指定从 HTTP 请求头 "api-version" 中读取版本号
    options.ApiVersionReader = new HeaderApiVersionReader("api-version");
});



// ======================= Swagger 配置 =======================
// 配置Swagger/OpenAPI文档生成服务
builder.Services.AddSwaggerGen(c =>
{
    // 定义Swagger文档信息
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "数据中台API",        // 文档标题
        Version = "v1",               // API 版本号
        Description = "数据中台接口文档" // 描述
    });

    // 获取当前程序集的XML文档文件路径
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // 如果XML文档文件存在，则包含XML注释
    if (File.Exists(xmlPath))
    {
        // 在Swagger UI 展示 控制器的注释，接口注释，参数注释等
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // 为Swagger添加JWT认证定义
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "使用 Bearer 方案的 JWT 授权标头。示例：\"Authorization: Bearer {token}\"",
        Name = "Authorization",            // 请求头参数名
        In = ParameterLocation.Header,     // 参数在请求头
        Type = SecuritySchemeType.ApiKey,  // 安全类型为 API Key
        Scheme = "Bearer"                  // 使用 Bearer 方案
    });

    // 添加安全要求，指定哪些端点需要JWT认证
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    // 引用之前定义的安全方案类型
                    Type = ReferenceType.SecurityScheme,
                    // 引用的安全方案ID
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});






// ======================= 注册自定义服务  开始 =======================
#region  注册自定义

// 注入 BasicInformationOfData  数据库
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BasicInformationOfData")));
// 注册仓储
builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();




// ================= Redis 配置 =================
// 绑定 appsettings.json Redis 节点到 RedisCacheOptions 对象
var redisOptions = new RedisCacheOptions();
builder.Configuration.GetSection("Redis").Bind(redisOptions);

// ================= 1️⃣ 注册 IDistributedCache =================
// 用于标准缓存操作（RedisService 将依赖它）
builder.Services.AddStackExchangeRedisCache(options =>
{
    // Redis 连接地址
    options.Configuration = redisOptions.RedisHost;

    // Redis Key 前缀
    options.InstanceName = redisOptions.InstanceName;
});

// ================= 2️⃣ RedisService 注入 =================
// 封装缓存逻辑（Set/Get、JSON 序列化、Key 前缀等）
builder.Services.AddScoped<IRedisService, RedisService>();

// 将 Redis 配置对象注入单例，方便其他服务使用
builder.Services.AddSingleton(redisOptions);

// ================= 3️⃣ 注册 IConnectionMultiplexer =================
// 用于分布式锁和发布订阅
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    // 构建连接字符串
    var config = redisOptions.RedisHost;
    if (!string.IsNullOrWhiteSpace(redisOptions.Password))
        config += $",password={redisOptions.Password}";

    // 建立 Redis 连接
    return ConnectionMultiplexer.Connect(config);
});

// ================= 4️⃣ 注册分布式锁和发布订阅服务 =================
builder.Services.AddScoped<DistributedLock>(); // 分布式锁
builder.Services.AddScoped<PubSubService>();   // 发布订阅服务




// ======================= 其他注入 =======================

builder.Services.AddScoped<IUserService, UserService>();   // 用户
builder.Services.AddScoped<IJwtService, JwtService>();




#endregion

// 添加 Prometheus 指标
builder.Services.AddHealthChecks(); // 内置健康检查


// ======================= 构建应用 =======================
var app = builder.Build();

// ======================= 使用中间件 =======================
app.UseMiddleware<RequestTrackingMiddleware>();


// ======================= 配置 HTTP 请求管道 =======================
// 开发环境启用 Swagger UI
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();       // 生成 Swagger JSON
    app.UseSwaggerUI();     // 提供可视化界面
//}

// ======================= 请求示例 =======================
app.MapGet("/api/hello", () => "Hello from WebAPI!");

// ======================= 临时下线接口 =======================
var tracker = app.Services.GetRequiredService<RequestTracker>();

app.MapGet("/disable", () =>
{
    tracker.Disable();
    return Results.Ok("Instance disabled");
}).ExcludeFromDescription();   // 👈 不显示在 Swagger;

app.MapGet("/enable", () =>
{
    tracker.Enable();
    return Results.Ok("Instance enabled");
}).ExcludeFromDescription();   // 👈 不显示在 Swagger;



// ======================= 健康检查接口 =======================
app.MapGet("/health", () =>
{
    if (!tracker.IsEnabled)
        return Results.StatusCode(503); // 禁用实例

    return Results.Ok("Healthy");
}).ExcludeFromDescription();   // 👈 不显示在 Swagger;








// 启用 HTTPS 重定向，将 HTTP 请求重定向到 HTTPS
//app.UseHttpsRedirection();

// 启用授权中间件，检查用户权限
app.UseAuthorization();


// 👇 必须放在认证后(中间件 - 用于检测当前用户session是否有效)
app.UseMiddleware<SingleLoginMiddleware>();

// 映射控制器路由到应用
app.MapControllers();

// 启动 Web 应用
app.Run();
