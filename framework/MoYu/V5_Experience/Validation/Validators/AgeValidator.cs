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
///     年龄（0-120 岁）验证器
/// </summary>
public class AgeValidator : ValidatorBase
{
    /// <summary>
    ///     <inheritdoc cref="AgeValidator" />
    /// </summary>
    public AgeValidator() => UseResourceKey(GetResourceKey);

    /// <summary>
    ///     是否仅允许成年人（18 岁及以上）
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    public bool IsAdultOnly { get; set; }

    /// <summary>
    ///     是否允许字符串数值
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    public bool AllowStringValues { get; set; }

    /// <inheritdoc />
    public override bool IsValid(object? value, IValidationContext? validationContext)
    {
        // 空检查
        if (value is null)
        {
            return true;
        }

        // 尝试将值解析为合法年龄（0-120）
        int? parsedAge = value switch
        {
            int i and >= 0 and <= 120 => i,
            uint u and <= 120 => (int)u,
            long l and >= 0 and <= 120 => (int)l,
            ulong ul and <= 120 => (int)ul,
            short s and >= 0 and <= 120 => s,
            ushort us and <= 120 => us,
            byte b => b,
            sbyte sb and >= 0 and <= 120 => sb,
            string str when AllowStringValues &&
                            int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var age) &&
                            age is >= 0 and <= 120 => age,
            _ => null
        };

        // 空检查
        if (parsedAge is null)
        {
            return false;
        }

        return !IsAdultOnly || parsedAge.Value >= 18;
    }

    /// <summary>
    ///     获取错误信息对应的资源键
    /// </summary>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal string GetResourceKey() =>
        IsAdultOnly
            ? nameof(ValidationMessages.AgeValidator_ValidationError_IsAdultOnly)
            : nameof(ValidationMessages.AgeValidator_ValidationError);
}