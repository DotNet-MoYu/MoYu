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

namespace MoYu.Extensions;

/// <summary>
///     <see cref="Task" /> 扩展类
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    ///     确保异步返回值不为 <code>null</code>
    /// </summary>
    /// <param name="task">
    ///     <see cref="Task" />
    /// </param>
    /// <typeparam name="T">异步类型</typeparam>
    /// <returns>
    ///     <typeparamref name="T" />
    /// </returns>
    public static async Task<T> ThrowIfNull<T>(this Task<T?> task)
    {
        var result = await task.ConfigureAwait(false);

        // 空检查
        ArgumentNullException.ThrowIfNull(result);

        return result;
    }

    /// <summary>
    ///     异步返回值为 <code>null</code> 时返回默认值
    /// </summary>
    /// <param name="task">
    ///     <see cref="Task" />
    /// </param>
    /// <typeparam name="T">异步类型</typeparam>
    /// <returns>
    ///     <typeparamref name="T" />
    /// </returns>
    public static Task<T> OrDefault<T>(this Task<T?> task) => task.OrDefault(default!);

    /// <summary>
    ///     异步返回值为 <code>null</code> 时返回默认值
    /// </summary>
    /// <param name="task">
    ///     <see cref="Task" />
    /// </param>
    /// <param name="defaultValue">默认值</param>
    /// <typeparam name="T">异步类型</typeparam>
    /// <returns>
    ///     <typeparamref name="T" />
    /// </returns>
    public static async Task<T> OrDefault<T>(this Task<T?> task, T defaultValue)
    {
        var result = await task.ConfigureAwait(false);

        return result ?? defaultValue;
    }
}