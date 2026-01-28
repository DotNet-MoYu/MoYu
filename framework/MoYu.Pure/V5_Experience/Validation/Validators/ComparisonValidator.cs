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

using MoYu.Validation.Resources;
using System.Globalization;

namespace MoYu.Validation;

/// <summary>
///     比较验证器抽象基类
/// </summary>
public abstract class ComparisonValidator : ValidatorBase
{
    /// <summary>
    ///     <inheritdoc cref="ComparisonValidator" />
    /// </summary>
    /// <param name="compareValue">比较的值</param>
    /// <param name="resourceKey">资源属性名</param>
    protected ComparisonValidator(IComparable compareValue, string resourceKey)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(compareValue);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceKey);

        CompareValue = compareValue;

        UseResourceKey(() => resourceKey);
    }

    /// <summary>
    ///     <inheritdoc cref="ComparisonValidator" />
    /// </summary>
    /// <param name="compareValue">比较的值</param>
    /// <param name="errorMessageResourceAccessor">错误信息资源访问器</param>
    protected ComparisonValidator(IComparable compareValue, Func<string> errorMessageResourceAccessor)
        : base(errorMessageResourceAccessor)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(compareValue);

        CompareValue = compareValue;
    }

    /// <summary>
    ///     比较的值
    /// </summary>
    public IComparable CompareValue { get; }

    /// <summary>
    ///     检查对象是否合法
    /// </summary>
    /// <param name="value">对象</param>
    /// <param name="validationContext">
    ///     <see cref="IValidationContext" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    protected abstract bool IsValid(IComparable value, IValidationContext? validationContext);

    /// <inheritdoc />
    public sealed override bool IsValid(object? value, IValidationContext? validationContext) =>
        value switch
        {
            null => true,
            IComparable val => IsValid(val, validationContext),
            _ => false
        };

    /// <inheritdoc />
    public override string FormatErrorMessage(string name) =>
        string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, CompareValue);

    /// <summary>
    ///     检查对象和比较的值是否为同一类型
    /// </summary>
    /// <remarks>用于确保比较逻辑的类型一致性。</remarks>
    /// <param name="value">对象</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    protected bool IsTypeMatchedToCompareValue(IComparable value) => value.GetType() == CompareValue.GetType();
}