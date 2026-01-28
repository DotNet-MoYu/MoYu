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
using System.Globalization;

namespace MoYu.Validation;

/// <summary>
///     最大长度验证器
/// </summary>
public class MaxLengthValidator : ValidatorBase
{
    /// <summary>
    ///     最大有效的长度值
    /// </summary>
    internal const int MaxAllowableLength = -1;

    /// <summary>
    ///     <inheritdoc cref="MaxLengthValidator" />
    /// </summary>
    /// <param name="length">最大允许长度</param>
    public MaxLengthValidator(int length)
    {
        Length = length;

        UseResourceKey(() => nameof(ValidationMessages.MaxLengthValidator_ValidationError));
    }

    /// <summary>
    ///     <inheritdoc cref="MaxLengthValidator" />
    /// </summary>
    public MaxLengthValidator()
        : this(MaxAllowableLength)
    {
    }

    /// <summary>
    ///     最大允许长度
    /// </summary>
    public int Length { get; }

    /// <inheritdoc />
    public override bool IsValid(object? value, IValidationContext? validationContext)
    {
        // 验证长度参数的合法性
        EnsureLegalLengths();

        // 空检查
        if (value == null)
        {
            return true;
        }

        // 尝试获取对象长度
        if (!value.TryGetCount(out var length))
        {
            throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture,
                "The field of type {0} must be a string, array or ICollection type.", value.GetType()));
        }

        return MaxAllowableLength == Length || length <= Length;
    }

    /// <inheritdoc />
    public override string FormatErrorMessage(string name) =>
        string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, Length);

    /// <summary>
    ///     验证长度参数的合法性
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    internal void EnsureLegalLengths()
    {
        if (Length is 0 or < -1)
        {
            throw new InvalidOperationException(
                "MaxLengthValidator must have a Length value that is greater than zero. Use MaxLength() without parameters to indicate that the string or array can have the maximum allowable length.");
        }
    }
}