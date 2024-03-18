// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;

namespace MoYu.DatabaseAccessor;

/// <summary>
/// 数据库上下文池
/// </summary>
public interface IDbContextPool
{
    /// <summary>
    /// 数据库上下文事务
    /// </summary>
    IDbContextTransaction DbContextTransaction { get; }

    /// <summary>
    /// 获取所有数据库上下文
    /// </summary>
    /// <returns></returns>
    ConcurrentDictionary<Guid, DbContext> GetDbContexts();

    /// <summary>
    /// 保存数据库上下文
    /// </summary>
    /// <param name="dbContext"></param>
    void AddToPool(DbContext dbContext);

    /// <summary>
    /// 保存数据库上下文（异步）
    /// </summary>
    /// <param name="dbContext"></param>
    Task AddToPoolAsync(DbContext dbContext);

    /// <summary>
    /// 保存数据库上下文池中所有已更改的数据库上下文
    /// </summary>
    /// <returns></returns>
    int SavePoolNow();

    /// <summary>
    /// 保存数据库上下文池中所有已更改的数据库上下文
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <returns></returns>
    int SavePoolNow(bool acceptAllChangesOnSuccess);

    /// <summary>
    /// 保存数据库上下文池中所有已更改的数据库上下文
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> SavePoolNowAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存数据库上下文池中所有已更改的数据库上下文（异步）
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> SavePoolNowAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);

    /// <summary>
    /// 打开事务
    /// </summary>
    /// <param name="ensureTransaction"></param>
    /// <returns></returns>
    void BeginTransaction(bool ensureTransaction = false);

    /// <summary>
    /// 提交事务
    /// </summary>
    /// <param name="withCloseAll">是否自动关闭所有连接</param>
    void CommitTransaction(bool withCloseAll = false);

    /// <summary>
    /// 回滚事务
    /// </summary>
    /// <param name="withCloseAll">是否自动关闭所有连接</param>
    void RollbackTransaction(bool withCloseAll = false);

    /// <summary>
    /// 关闭所有数据库链接
    /// </summary>
    void CloseAll();
}