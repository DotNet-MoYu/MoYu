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

using MoYu.Shapeless;
using MoYu.Shapeless.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     流变对象模块 <see cref="IMvcBuilder" /> 扩展类
/// </summary>
public static class ShapelessMvcBuilderExtensions
{
    /// <summary>
    ///     添加 <see cref="Clay" /> 配置
    /// </summary>
    /// <param name="builder">
    ///     <see cref="IMvcBuilder" />
    /// </param>
    /// <returns>
    ///     <see cref="IMvcBuilder" />
    /// </returns>
    public static IMvcBuilder AddClayOptions(this IMvcBuilder builder) => builder.AddClayOptions(_ => { });

    /// <summary>
    ///     添加 <see cref="Clay" /> 配置
    /// </summary>
    /// <param name="builder">
    ///     <see cref="IMvcBuilder" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="IMvcBuilder" />
    /// </returns>
    public static IMvcBuilder AddClayOptions(this IMvcBuilder builder, Action<ClayOptions> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        // 配置 JsonOptions 选项，添加 ClayJsonConverter 和 ObjectToClayJsonConverter 转换器
        builder.Services.Configure<JsonOptions>(options => options.JsonSerializerOptions.AddClayConverters());

        // 配置 ClayOptions 选项服务
        builder.Services.Configure(configure);

        // 添加 Clay 模型绑定提供器
        builder.Services.Configure<MvcOptions>(options =>
        {
            if (!options.ModelBinderProviders.OfType<ClayBinderProvider>().Any())
            {
                options.ModelBinderProviders.Insert(0, new ClayBinderProvider());
            }
        });

        return builder;
    }
}