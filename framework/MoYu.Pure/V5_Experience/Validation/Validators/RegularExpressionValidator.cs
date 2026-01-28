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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MoYu.Validation;

/// <summary>
///     正则表达式验证器
/// </summary>
public class RegularExpressionValidator : ValidatorBase
{
    /// <summary>
    ///     <inheritdoc cref="RegularExpressionValidator" />
    /// </summary>
    /// <param name="pattern">正则表达式模式</param>
    public RegularExpressionValidator([StringSyntax(StringSyntaxAttribute.Regex)] string pattern)
    {
        Pattern = pattern;
        MatchTimeoutInMilliseconds = 2000;

        UseResourceKey(() => nameof(ValidationMessages.RegularExpressionValidator_ValidationError));
    }

    /// <summary>
    ///     正则表达式模式
    /// </summary>
    public string Pattern { get; }

    /// <summary>
    ///     用于在操作超时前执行单个匹配操作的时间量
    /// </summary>
    /// <remarks>以毫秒为单位，默认值为：2000。</remarks>
    public int MatchTimeoutInMilliseconds { get; set; }

    /// <summary>
    ///     匹配正则表达式模式时要使用的超时值
    /// </summary>
    public TimeSpan MatchTimeout => TimeSpan.FromMilliseconds(MatchTimeoutInMilliseconds);

    /// <summary>
    ///     缓存正则表达式 <see cref="Regex" /> 实例
    /// </summary>
    internal Regex? Regex { get; set; }

    /// <inheritdoc />
    public override bool IsValid(object? value, IValidationContext? validationContext)
    {
        // 确保 Regex 实例已初始化
        SetupRegex();

        // 将对象转换为字符串
        var stringValue = Convert.ToString(value, CultureInfo.CurrentCulture);

        // 空检查
        if (string.IsNullOrEmpty(stringValue))
        {
            return true;
        }

        // 使用 EnumerateMatches 遍历所有匹配项
        foreach (var valueMatch in Regex!.EnumerateMatches(stringValue))
        {
            // 判断是否完全匹配
            return valueMatch.Index == 0 && valueMatch.Length == stringValue.Length;
        }

        return false;
    }

    /// <inheritdoc />
    public override string FormatErrorMessage(string name)
    {
        // 确保 Regex 实例已初始化
        SetupRegex();

        return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, Pattern);
    }

    /// <summary>
    ///     初始化 <see cref="Regex" /> 实例
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    internal void SetupRegex()
    {
        // 空检查
        if (Regex is not null)
        {
            return;
        }

        // 空检查
        if (string.IsNullOrEmpty(Pattern))
        {
            throw new InvalidOperationException("The pattern must be set to a valid regular expression.");
        }

        Regex = MatchTimeoutInMilliseconds == -1
            ? new Regex(Pattern)
            : new Regex(Pattern, default, TimeSpan.FromMilliseconds(MatchTimeoutInMilliseconds));
    }
}