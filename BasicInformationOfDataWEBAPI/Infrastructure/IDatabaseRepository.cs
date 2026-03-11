using System.Linq.Expressions;

namespace BasicInformationOfDataWEBAPI.Infrastructure
{
    /// <summary>
    /// 数据库访问仓储接口
    /// 统一封装常用 CRUD 操作
    /// Service 层通过接口访问数据库，实现业务逻辑与具体 ORM/数据库解耦
    /// </summary>
    public interface IDatabaseRepository
    {
        /// <summary>
        /// 根据主键获取实体
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="id">主键</param>
        /// <returns>实体对象或 null</returns>
        Task<TEntity?> GetByIdAsync<TEntity>(object id) where TEntity : class;

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns>实体集合</returns>
        Task<IEnumerable<TEntity>> GetAllAsync<TEntity>() where TEntity : class;

        /// <summary>
        /// 根据条件获取实体集合
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="predicate">查询条件表达式</param>
        /// <returns>符合条件的实体集合</returns>
        Task<IEnumerable<TEntity>> QueryAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;

        /// <summary>
        /// 新增实体
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="entity">要新增的实体对象</param>
        /// <returns>异步任务</returns>
        Task AddAsync<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="entity">要更新的实体对象</param>
        /// <returns>异步任务</returns>
        Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="entity">要删除的实体对象</param>
        /// <returns>异步任务</returns>
        Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// 保存所有更改（如果使用 Unit of Work 或 DbContext）
        /// </summary>
        /// <returns>异步任务，返回受影响行数</returns>
        Task<int> SaveChangesAsync();
    }
}