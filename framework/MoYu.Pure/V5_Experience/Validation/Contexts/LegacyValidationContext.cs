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

namespace MoYu.Validation;

/// <inheritdoc />
/// <remarks>用于兼容旧版本 <see cref="ValidatorBase" /> 相关方法。</remarks>
internal sealed class LegacyValidationContext : IValidationContext
{
    /// <summary>
    ///     <inheritdoc cref="LegacyValidationContext" />
    /// </summary>
    internal LegacyValidationContext()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="LegacyValidationContext" />
    /// </summary>
    /// <param name="instance">对象</param>
    /// <param name="displayName">显示名称</param>
    /// <param name="memberNames">成员名称列表</param>
    internal LegacyValidationContext(object? instance, string? displayName, IEnumerable<string>? memberNames)
    {
        Instance = instance;
        DisplayName = displayName!;
        MemberNames = memberNames;
    }

    /// <inheritdoc />
    public object? Instance { get; set; }

    /// <inheritdoc />
    public string DisplayName { get => field ?? MemberNames?.FirstOrDefault() ?? Instance?.GetType().Name!; set; }

    /// <inheritdoc />
    public IEnumerable<string>? MemberNames { get; set; }

    /// <inheritdoc />
    public string?[]? RuleSets { get; set; }

    /// <inheritdoc />
    public IDictionary<object, object?> Items { get; set; } = new Dictionary<object, object?>();

    /// <inheritdoc />
    public object? GetService(Type serviceType) => null;
}