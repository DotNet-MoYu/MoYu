﻿// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

namespace MoYu.DatabaseAccessor;

/// <summary>
/// 可写仓储分部类
/// </summary>
public partial class PrivateRepository<TEntity>
    where TEntity : class, IPrivateEntity, new()
{
    /// <summary>
    /// 接受所有更改
    /// </summary>
    public virtual void AcceptAllChanges()
    {
        ChangeTracker.AcceptAllChanges();
    }

    /// <summary>
    /// 保存数据库上下文池中所有已更改的数据库上下文
    /// </summary>
    /// <returns></returns>
    public int SavePoolNow()
    {
        return _dbContextPool.SavePoolNow();
    }

    /// <summary>
    /// 保存数据库上下文池中所有已更改的数据库上下文
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <returns></returns>
    public int SavePoolNow(bool acceptAllChangesOnSuccess)
    {
        return _dbContextPool.SavePoolNow(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// 保存数据库上下文池中所有已更改的数据库上下文
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<int> SavePoolNowAsync(CancellationToken cancellationToken = default)
    {
        return _dbContextPool.SavePoolNowAsync(cancellationToken);
    }

    /// <summary>
    /// 保存数据库上下文池中所有已更改的数据库上下文
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<int> SavePoolNowAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        return _dbContextPool.SavePoolNowAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// 提交更改操作
    /// </summary>
    /// <returns></returns>
    public virtual int SaveNow()
    {
        return Context.SaveChanges();
    }

    /// <summary>
    /// 提交更改操作
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <returns></returns>
    public virtual int SaveNow(bool acceptAllChangesOnSuccess)
    {
        return Context.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// 提交更改操作（异步）
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task<int> SaveNowAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 提交更改操作（异步）
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task<int> SaveNowAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}