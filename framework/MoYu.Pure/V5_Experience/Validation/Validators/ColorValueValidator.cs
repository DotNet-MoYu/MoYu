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
using System.Text.RegularExpressions;

namespace MoYu.Validation;

/// <summary>
///     颜色值验证器
/// </summary>
public partial class ColorValueValidator : ValidatorBase
{
    /// <summary>
    ///     <inheritdoc cref="ColorValueValidator" />
    /// </summary>
    public ColorValueValidator() =>
        UseResourceKey(() => nameof(ValidationMessages.ColorValueValidator_ValidationError));

    /// <summary>
    ///     是否启用完整模式
    /// </summary>
    /// <remarks>在完整模式下，支持的颜色格式包括：十六进制颜色、RGB、RGBA、HSL 和 HSLA；若未启用，则仅支持十六进制颜色、RGB 和 RGBA。默认值为：<c>false</c>。</remarks>
    public bool FullMode { get; set; }

    /// <inheritdoc />
    public override bool IsValid(object? value, IValidationContext? validationContext) =>
        value switch
        {
            null => true,
            string text => !string.IsNullOrWhiteSpace(text) && (FullMode ? Regex() : StandardRegex()).IsMatch(text),
            _ => false
        };

    /// <summary>
    ///     颜色值正则表达式（完整模式）
    /// </summary>
    /// <returns>
    ///     <see cref="System.Text.RegularExpressions.Regex" />
    /// </returns>
    [GeneratedRegex(
        @"^(?:#(?:(?:[0-9a-fA-F]{3}){1,2}|[0-9a-fA-F]{8})|rgba?\((?:\s*\d+\%?\s*,){2}\s*(?:\d+\%?\s*(?:,\s*[0-9.]+\s*)?)?\)|hsla?\((?:\s*\d+\%?\s*,){2}\s*(?:\d+\%?\s*(?:,\s*[0-9.]+\s*)?)?\)|hwb\((?:\s*\d+\%?\s*,){2}\s*(?:\d+\%?\s*)?\)|lch\((?:\s*\d+\%?\s*,){2}\s*(?:\d+\%?\s*)?\)|oklch\((?:\s*\d+\%?\s*,){2}\s*(?:\d+\%?\s*)?\)|lab\((?:\s*[-+]?\d+\%?\s*,){2}\s*[-+]?\d+\%?\s*\)|oklab\((?:\s*[-+]?\d+\%?\s*,){2}\s*[-+]?\d+\%?\s*\))$",
        RegexOptions.IgnoreCase)]
    private static partial Regex Regex();

    /// <summary>
    ///     颜色值正则表达式（标准模式）
    /// </summary>
    /// <remarks>仅支持：十六进制颜色、RGB 和 RGBA。</remarks>
    /// <returns>
    ///     <see cref="System.Text.RegularExpressions.Regex" />
    /// </returns>
    [GeneratedRegex(
        @"^(?:#(?:[0-9a-fA-F]{3}){1,2}|rgba?\((?:\s*(?:\d+%?)\s*,){2}\s*(?:\d+%?)\s*(?:,\s*(?:\d+(?:\.\d+)?|\.\d+))?\))$",
        RegexOptions.IgnoreCase)]
    private static partial Regex StandardRegex();
}