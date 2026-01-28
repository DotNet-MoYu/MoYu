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

using MoYu.Validation;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     数据验证模块 <see cref="IServiceCollection" /> 扩展类
/// </summary>
public static class ValidationCoreServiceCollectionExtensions
{
    /// <summary>
    ///     添加数据验证服务
    /// </summary>
    /// <param name="services">
    ///     <see cref="IServiceCollection" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="IServiceCollection" />
    /// </returns>
    public static IServiceCollection AddValidationCore(this IServiceCollection services,
        Action<ValidationBuilder>? configure = null)
    {
        // 初始化数据验证构建器
        var validationBuilder = new ValidationBuilder();

        // 调用自定义配置委托
        configure?.Invoke(validationBuilder);

        return services.AddValidationCore(validationBuilder);
    }

    /// <summary>
    ///     添加数据验证服务
    /// </summary>
    /// <param name="services">
    ///     <see cref="IServiceCollection" />
    /// </param>
    /// <param name="validationBuilder">
    ///     <see cref="ValidationBuilder" />
    /// </param>
    /// <returns>
    ///     <see cref="IServiceCollection" />
    /// </returns>
    public static IServiceCollection AddValidationCore(this IServiceCollection services,
        ValidationBuilder validationBuilder)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(validationBuilder);

        // 构建模块服务
        validationBuilder.Build(services);

        return services;
    }
}