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
///     HTTP 远程请求服务构建器
/// </summary>
public interface IHttpRemoteBuilder
{
    /// <summary>
    ///     <see cref="IServiceCollection" />
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    ///     配置 <see cref="HttpRemoteOptions" /> 实例
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="IHttpRemoteBuilder" />
    /// </returns>
    IHttpRemoteBuilder ConfigureOptions(Action<HttpRemoteOptions> configure);

    /// <summary>
    ///     配置 <see cref="HttpRemoteOptions" /> 实例
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="IHttpRemoteBuilder" />
    /// </returns>
    IHttpRemoteBuilder ConfigureOptions(Action<HttpRemoteOptions, IServiceProvider> configure);

    /// <summary>
    ///     为所有 <see cref="HttpClient" /> 添加配置
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="IHttpRemoteBuilder" />
    /// </returns>
    IHttpRemoteBuilder ConfigureHttpClientDefaults(Action<IHttpClientBuilder> configure);
}

/// <summary>
///     <see cref="IHttpRemoteBuilder" /> 默认实现
/// </summary>
internal sealed class DefaultHttpRemoteBuilder : IHttpRemoteBuilder
{
    /// <summary>
    ///     <inheritdoc cref="DefaultHttpRemoteBuilder" />
    /// </summary>
    /// <param name="services">
    ///     <see cref="IServiceCollection" />
    /// </param>
    public DefaultHttpRemoteBuilder(IServiceCollection services)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(services);

        Services = services;
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <inheritdoc />
    public IHttpRemoteBuilder ConfigureOptions(Action<HttpRemoteOptions> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        Services.Configure(configure);

        return this;
    }

    /// <inheritdoc />
    public IHttpRemoteBuilder ConfigureOptions(Action<HttpRemoteOptions, IServiceProvider> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        Services.AddOptions<HttpRemoteOptions>().Configure(configure);

        return this;
    }

    /// <inheritdoc />
    public IHttpRemoteBuilder ConfigureHttpClientDefaults(Action<IHttpClientBuilder> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        Services.ConfigureHttpClientDefaults(configure);

        return this;
    }
}