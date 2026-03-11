using BasicInformationOfDataWEBAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BasicInformationOfDataWEBAPI.Infrastructure
{
    /// <summary>
    /// 应用程序数据库上下文类
    /// 
    /// 继承自 EF Core 的 DbContext，用于封装与数据库的所有操作。
    /// Service 层通过 IDatabaseRepository 接口访问数据库，不直接依赖 DbContext。
    /// Infrastructure 层负责管理 DbContext 的生命周期和实体映射。
    /// </summary>
    /// <remarks>
    /// 构造函数，接收 DbContextOptions 配置
    /// </remarks>
    /// <param name="options">
    /// DbContext 配置选项（例如数据库连接字符串、数据库提供程序）
    /// 由依赖注入在 Program.cs 中提供
    /// </param>
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {

        /// <summary>
        /// 用户表的 DbSet 集合
        /// 
        /// EF Core 会根据 DbSet 的泛型类型生成数据库表。
        /// Users 对应数据库中的用户表。
        /// 
        /// 注意：
        /// - DbSet 属性应为 public，以便仓储层访问
        /// - 使用 null! 是为了消除 C# 8 nullable 警告，EF Core 会在运行时自动初始化
        /// </summary>
        public DbSet<Simsuserinfo> Simsuserinfo { get; set; } = null!;






        /*
        // 如果有更多实体表，可以继续添加 DbSet：
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        */

        /// <summary>
        /// 可选：重写 OnModelCreating 配置实体映射
        /// </summary>
        /// <param name="modelBuilder">模型构建器</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 在这里可以做 Fluent API 配置，例如：
            // modelBuilder.Entity<User>()
            //     .HasKey(u => u.Id); // 配置主键
            //
            // modelBuilder.Entity<User>()
            //     .Property(u => u.Name)
            //     .IsRequired()
            //     .HasMaxLength(50);
        }
    }
}