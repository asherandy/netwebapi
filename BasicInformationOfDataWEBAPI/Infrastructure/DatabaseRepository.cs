using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BasicInformationOfDataWEBAPI.Infrastructure
{
    /// <summary>
    /// 通用数据库仓储实现类
    /// 封装 EF Core 对数据库的 CRUD 操作
    /// 属于 Infrastructure 层，Service 层通过 IDatabaseRepository 接口访问数据库
    /// </summary>
    public class DatabaseRepository : IDatabaseRepository
    {
        /// <summary>
        /// EF Core 数据上下文
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// 构造函数，注入 AppDbContext
        /// </summary>
        /// <param name="context">数据库上下文，由依赖注入提供</param>
        public DatabaseRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据主键获取单个实体
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="id">实体主键</param>
        /// <returns>返回实体对象，如果未找到则返回 null</returns>
        public async Task<TEntity?> GetByIdAsync<TEntity>(object id) where TEntity : class
        {
            // EF Core 的 FindAsync 会根据主键查找实体
            return await _context.Set<TEntity>().FindAsync(id);
        }

        /// <summary>
        /// 获取指定实体类型的所有记录
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns>实体集合</returns>
        public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>() where TEntity : class
        {
            // 使用 DbSet.ToListAsync 获取所有记录
            return await _context.Set<TEntity>().ToListAsync();
        }

        /// <summary>
        /// 根据条件查询实体集合
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="predicate">查询条件表达式</param>
        /// <returns>满足条件的实体集合</returns>
        public async Task<IEnumerable<TEntity>> QueryAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            // EF Core 的 Where + ToListAsync 查询符合条件的记录
            return await _context.Set<TEntity>().Where(predicate).ToListAsync();
        }

        /// <summary>
        /// 新增实体
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="entity">要新增的实体对象</param>
        /// <returns>异步任务</returns>
        public async Task AddAsync<TEntity>(TEntity entity) where TEntity : class
        {
            // EF Core 的 AddAsync 方法添加实体到上下文
            await _context.Set<TEntity>().AddAsync(entity);
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="entity">要更新的实体对象</param>
        /// <returns>异步任务</returns>
        public async Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class
        {
            // EF Core 的 Update 方法将实体标记为已修改
            _context.Set<TEntity>().Update(entity);
            await Task.CompletedTask; // 保持异步签名
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="entity">要删除的实体对象</param>
        /// <returns>异步任务</returns>
        public async Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class
        {
            // EF Core 的 Remove 方法将实体标记为删除
            _context.Set<TEntity>().Remove(entity);
            await Task.CompletedTask; // 保持异步签名
        }

        /// <summary>
        /// 提交数据库更改
        /// </summary>
        /// <returns>返回受影响的行数</returns>
        public async Task<int> SaveChangesAsync()
        {
            // EF Core 提交所有已添加、修改、删除的实体
            return await _context.SaveChangesAsync();
        }
    }
}