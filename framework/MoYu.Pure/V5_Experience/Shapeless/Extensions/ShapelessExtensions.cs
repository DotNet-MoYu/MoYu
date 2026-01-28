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

namespace MoYu.Shapeless.Extensions;

/// <summary>
///     流变对象模块扩展类
/// </summary>
public static class ShapelessExtensions
{
    /// <summary>
    ///     将对象转换为 <see cref="Clay" /> 实例
    /// </summary>
    /// <param name="obj">
    ///     <see cref="object" />
    /// </param>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay ToClay(this object? obj, ClayOptions? options = null) => Clay.Parse(obj, options);

    /// <summary>
    ///     将对象转换为 <see cref="Clay" /> 实例
    /// </summary>
    /// <param name="obj">
    ///     <see cref="object" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay ToClay(this object? obj, Action<ClayOptions> configure) => Clay.Parse(obj, configure);

    /// <summary>
    ///     将 <see cref="Clay" /> 实例通过转换管道传递并返回新的 <see cref="Clay" />（失败时抛出异常）
    /// </summary>
    /// <param name="clayTask">
    ///     <see cref="Task{TResult}" />
    /// </param>
    /// <param name="transformer">转换函数</param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<Clay?> PipeAsync(this Task<Clay?> clayTask, Func<dynamic, dynamic?> transformer)
    {
        var clay = await clayTask;
        return clay?.Pipe(transformer);
    }

    /// <summary>
    ///     尝试将 <see cref="Clay" /> 实例通过转换管道传递，失败时返回原始对象
    /// </summary>
    /// <param name="clayTask">
    ///     <see cref="Task{TResult}" />
    /// </param>
    /// <param name="transformer">returns</param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static async Task<Clay?> PipeTryAsync(this Task<Clay?> clayTask, Func<dynamic, dynamic?> transformer)
    {
        var clay = await clayTask;
        return clay?.PipeTry(transformer);
    }
}