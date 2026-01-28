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
///     银行卡号验证器（Luhn 算法）
/// </summary>
/// <remarks>
///     <see href="https://baike.baidu.com/item/Luhn算法/22799984">Luhn 算法</see>
///     <see href="https://www.ee.unb.ca/cgi-bin/tervo/luhn.pl">Luhn 算法在线测试</see>
/// </remarks>
public partial class BankCardValidator : ValidatorBase
{
    /// <summary>
    ///     <inheritdoc cref="BankCardValidator" />
    /// </summary>
    public BankCardValidator() => UseResourceKey(() => nameof(ValidationMessages.BankCardValidator_ValidationError));

    /// <inheritdoc />
    public override bool IsValid(object? value, IValidationContext? validationContext)
    {
        // 空检查
        if (value is null)
        {
            return true;
        }

        // 清理输入：去除空格/特殊字符（如 '-' 或空格）
        var sanitized = value switch
        {
            string s => s.Replace(" ", "").Replace("-", ""),
            _ => value.ToString()?.Replace(" ", "").Replace("-", "")
        };

        // 格式验证 + Luhn 算法校验
        return !string.IsNullOrWhiteSpace(sanitized) && Regex().IsMatch(sanitized) && CheckLuhn(sanitized);
    }

    /// <summary>
    ///     Luhn 算法校验
    /// </summary>
    /// <param name="number">卡号</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool CheckLuhn(string number)
    {
        var sum = 0;
        var length = number.Length;

        for (var i = 0; i < length; i++)
        {
            var add = (number[i] - '0') * (2 - ((i + length) % 2));
            add -= add > 9 ? 9 : 0;
            sum += add;
        }

        return sum % 10 == 0;
    }

    /// <summary>
    ///     银行卡号正则表达式
    /// </summary>
    /// <returns>
    ///     <see cref="System.Text.RegularExpressions.Regex" />
    /// </returns>
    [GeneratedRegex(@"^[1-9]\d{11,18}$")]
    private static partial Regex Regex();
}