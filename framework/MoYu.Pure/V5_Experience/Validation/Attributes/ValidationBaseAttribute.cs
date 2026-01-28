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

using MoYu.Extensions;
using MoYu.Validation.Resources;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MoYu.Validation;

/// <summary>
///     验证特性抽象基类
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public abstract class ValidationBaseAttribute : ValidationAttribute
{
    /// <summary>
    ///     ErrorMessageResourceAccessor 属性设置器
    /// </summary>
    internal static readonly Lazy<Action<object, object?>> _errorMessageResourceAccessorSetter = new(() =>
        typeof(ValidationAttribute).CreatePropertySetter(
            typeof(ValidationAttribute).GetProperty("ErrorMessageResourceAccessor",
                BindingFlags.Instance | BindingFlags.NonPublic)!));

    /// <inheritdoc />
    protected ValidationBaseAttribute()
    {
    }

    /// <inheritdoc />
    protected ValidationBaseAttribute(string errorMessage)
        : base(errorMessage)
    {
    }

    /// <inheritdoc />
    protected ValidationBaseAttribute(Func<string> errorMessageAccessor)
        : base(errorMessageAccessor)
    {
    }

    /// <summary>
    ///     使用指定资源键设置验证错误信息
    /// </summary>
    /// <remarks>支持入口程序集覆盖框架内部资源，若未找到则返回占位符。</remarks>
    /// <param name="resourceKeyResolver">返回 <see cref="ValidationMessages" /> 中属性名的委托</param>
    protected void UseResourceKey(Func<string> resourceKeyResolver)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(resourceKeyResolver);

        // 创建委托的本地副本
        var capturedResourceKeyResolver = resourceKeyResolver;

        _errorMessageResourceAccessorSetter.Value(this, () =>
        {
            // 获取 ValidationMessages 中的属性名
            var resourceKey = capturedResourceKeyResolver();

            return ValidatorBase.GetResourceString(resourceKey) ?? resourceKey;
        });
    }
}