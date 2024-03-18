// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace MoYu.DatabaseAccessor;

/// <summary>
/// 常量、公共方法配置类
/// </summary>
internal static class Penetrates
{
    /// <summary>
    /// 数据库上下文描述器
    /// </summary>
    internal static readonly ConcurrentDictionary<Type, Type> DbContextDescriptors;

    /// <summary>
    /// 构造函数
    /// </summary>
    static Penetrates()
    {
        DbContextDescriptors = new ConcurrentDictionary<Type, Type>();
    }

    /// <summary>
    /// 配置 SqlServer 数据库上下文
    /// </summary>
    /// <param name="optionBuilder">数据库上下文选项构建器</param>
    /// <param name="interceptors">拦截器</param>
    /// <returns></returns>
    internal static Action<IServiceProvider, DbContextOptionsBuilder> ConfigureDbContext(Action<IServiceProvider, DbContextOptionsBuilder> optionBuilder, params IInterceptor[] interceptors)
    {
        return (serviceProvider, options) =>
        {
            // 只有开发环境开启
            if (App.HostEnvironment?.IsDevelopment() ?? false)
            {
                options/*.UseLazyLoadingProxies()*/
                         .EnableDetailedErrors()
                         .EnableSensitiveDataLogging();
            }

            optionBuilder?.Invoke(serviceProvider, options);

            // 添加拦截器
            AddInterceptors(interceptors, options);
        };
    }

    /// <summary>
    /// 检查数据库上下文是否绑定
    /// </summary>
    /// <param name="dbContextLocatorType"></param>
    /// <param name="dbContextType"></param>
    /// <returns></returns>
    internal static void CheckDbContextLocator(Type dbContextLocatorType, out Type dbContextType)
    {
        if (!DbContextDescriptors.TryGetValue(dbContextLocatorType, out dbContextType)) throw new InvalidCastException($" The dbcontext locator `{dbContextLocatorType.Name}` is not bind.");
    }

    /// <summary>
    /// 数据库数据库拦截器
    /// </summary>
    /// <param name="interceptors">拦截器</param>
    /// <param name="options"></param>
    private static void AddInterceptors(IInterceptor[] interceptors, DbContextOptionsBuilder options)
    {
        // 添加拦截器
        var interceptorList = DbProvider.GetDefaultInterceptors();

        if (interceptors != null || interceptors.Length > 0)
        {
            interceptorList.AddRange(interceptors);
        }
        options.AddInterceptors(interceptorList.ToArray());
    }
}