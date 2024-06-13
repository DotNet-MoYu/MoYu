﻿// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using Microsoft.Extensions.DependencyInjection;

namespace MoYu.DatabaseAccessor;

/// <summary>
/// 多数据库仓储
/// </summary>
/// <typeparam name="TDbContextLocator"></typeparam>
public partial class DbRepository<TDbContextLocator> : IDbRepository<TDbContextLocator>
    where TDbContextLocator : class, IDbContextLocator
{
    /// <summary>
    /// 服务提供器
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    public DbRepository(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 切换实体
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public virtual IRepository<TEntity, TDbContextLocator> Change<TEntity>()
         where TEntity : class, IPrivateEntity, new()
    {
        return _serviceProvider.GetService<IRepository<TEntity, TDbContextLocator>>();
    }

    /// <summary>
    /// 获取 Sql 操作仓储
    /// </summary>
    /// <returns></returns>
    public virtual ISqlRepository<TDbContextLocator> Sql()
    {
        return _serviceProvider.GetService<ISqlRepository<TDbContextLocator>>();
    }

    /// <summary>
    /// 解析服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public virtual TService GetService<TService>()
    {
        return _serviceProvider.GetService<TService>();
    }

    /// <summary>
    /// 解析服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public virtual TService GetRequiredService<TService>()
    {
        return _serviceProvider.GetRequiredService<TService>();
    }
}