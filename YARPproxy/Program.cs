var builder = WebApplication.CreateBuilder(args);

// 添加 YARP 反向代理服务
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
// 绑定所有 IP
builder.WebHost.UseUrls("http://0.0.0.0:5002");


// 添加健康检查服务
builder.Services.AddHealthChecks();

var app = builder.Build();
// 映射 YARP 代理
app.MapReverseProxy();


// 配置中间件
app.MapHealthChecks("/health");




app.Run();
