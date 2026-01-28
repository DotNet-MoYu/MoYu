// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// MoYu 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// 官方网站：https://MoYu.net
//
// 使用条款
// 使用本代码应遵守相关法律法规和许可证的要求。
//
// 免责声明
// 对于因使用本代码而产生的任何直接、间接、偶然、特殊或后果性损害，我们不承担任何责任。
//
// 其他重要信息
// MoYu 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。
// 有关 MoYu 项目的其他详细信息，请参阅位于源代码树根目录中的 COPYRIGHT 和 DISCLAIMER 文件。
//
// 更多信息
// 请访问 https://gitee.com/dotnetchina/MoYu 获取更多关于 MoYu 项目的许可证和版权信息。
// ------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

namespace MoYu.HttpRemote;

/// <summary>
///     提供静态访问 <see cref="IHttpRemoteService" /> 服务的方式
/// </summary>
/// <remarks>支持服务的延迟初始化、配置更新以及资源释放。</remarks>
public static class HttpRemoteClient
{
    /// <inheritdoc cref="IServiceProvider" />
    internal static IServiceProvider? _serviceProvider;

    /// <summary>
    ///     延迟加载的 <see cref="IHttpRemoteService" /> 实例
    /// </summary>
    internal static Lazy<IHttpRemoteService> _httpRemoteService;

    /// <summary>
    ///     并发锁对象
    /// </summary>
    internal static readonly SemaphoreSlim _initializationLock = new(1, 1);

    /// <summary>
    ///     标记服务是否已释放
    /// </summary>
    internal static bool _isDisposed;

    /// <summary>
    ///     自定义服务注册逻辑的委托
    /// </summary>
    internal static Action<IServiceCollection> _configure = services => services.AddHttpRemote();

    /// <summary>
    ///     <inheritdoc cref="HttpRemoteClient" />
    /// </summary>
    static HttpRemoteClient() => _httpRemoteService = new Lazy<IHttpRemoteService>(CreateService);

    /// <summary>
    ///     获取当前配置下的 <see cref="IHttpRemoteService" /> 实例
    /// </summary>
    public static IHttpRemoteService Service
    {
        get
        {
            // 释放检查
            ObjectDisposedException.ThrowIf(_isDisposed, typeof(HttpRemoteClient));

            return _httpRemoteService.Value;
        }
    }

    /// <summary>
    ///     自定义服务注册逻辑
    /// </summary>
    public static void Configure(Action<IServiceCollection> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        // 初始化锁
        _initializationLock.Wait();

        try
        {
            // 释放检查
            ObjectDisposedException.ThrowIf(_isDisposed, typeof(HttpRemoteClient));

            // 更新配置委托
            _configure = services =>
            {
                // 调用自定义配置委托
                configure(services);

                // 检查 HTTP 远程请求服务是否已注册，若未注册则自动完成注册
                if (services.All(u => u.ServiceType != typeof(IHttpRemoteService)))
                {
                    services.AddHttpRemote();
                }
            };

            // 重新初始化服务
            Reinitialize();
        }
        finally
        {
            // 释放锁
            _initializationLock.Release();
        }
    }

    /// <summary>
    ///     释放服务提供器及相关资源
    /// </summary>
    /// <remarks>通常在应用程序关闭或不再需要 HTTP 远程请求服务时调用。</remarks>
    public static void Dispose()
    {
        // 初始化锁
        _initializationLock.Wait();

        try
        {
            // 幂等性处理
            if (_isDisposed)
            {
                return;
            }

            // 重新初始化服务
            Reinitialize();

            // 标记为已释放状态
            _isDisposed = true;
        }
        finally
        {
            // 释放锁
            _initializationLock.Release();
        }
    }

    /// <summary>
    ///     创建 <see cref="IHttpRemoteService" /> 实例
    /// </summary>
    /// <returns>
    ///     <see cref="IHttpRemoteService" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static IHttpRemoteService CreateService()
    {
        // 初始化锁
        _initializationLock.Wait();

        try
        {
            // 释放检查
            ObjectDisposedException.ThrowIf(_isDisposed, typeof(HttpRemoteClient));

            // 如果值已创建，直接返回
            if (_httpRemoteService.IsValueCreated)
            {
                return _httpRemoteService.Value;
            }

            try
            {
                // 初始化 ServiceCollection 实例
                var services = new ServiceCollection();

                // 调用自定义服务注册逻辑的委托
                _configure(services);

                // 构建服务提供器
                _serviceProvider = services.BuildServiceProvider();

                // 解析 IHttpRemoteService 实例并返回
                return _serviceProvider.GetRequiredService<IHttpRemoteService>();
            }
            catch (Exception ex)
            {
                // 重新初始化服务
                Reinitialize();

                throw new InvalidOperationException("Failed to initialize IHttpRemoteService.", ex);
            }
        }
        finally
        {
            // 释放锁
            _initializationLock.Release();
        }
    }

    /// <summary>
    ///     使用最新的 <see cref="Configure" /> 配置重新初始化服务
    /// </summary>
    internal static void Reinitialize()
    {
        // 释放检查
        ObjectDisposedException.ThrowIf(_isDisposed, typeof(HttpRemoteClient));

        // 释放旧资源
        ReleaseServiceProvider();

        // 重置延迟加载实例
        _httpRemoteService = new Lazy<IHttpRemoteService>(CreateService);
    }

    /// <summary>
    ///     释放服务提供器
    /// </summary>
    internal static void ReleaseServiceProvider()
    {
        // 如果服务提供器支持释放资源，则执行释放操作
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _serviceProvider = null;
    }
}