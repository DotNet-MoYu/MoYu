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
using System.Text.RegularExpressions;

namespace MoYu.Validation;

/// <summary>
///     域名验证器
/// </summary>
/// <remarks>不含协议（如 https/http）。</remarks>
public partial class DomainValidator : ValidatorBase
{
    /// <inheritdoc cref="IdnMapping" />
    internal readonly IdnMapping _idnMapping;

    /// <summary>
    ///     <inheritdoc cref="DomainValidator" />
    /// </summary>
    public DomainValidator()
    {
        _idnMapping = new IdnMapping { AllowUnassigned = true };

        UseResourceKey(() => nameof(ValidationMessages.DomainValidator_ValidationError));
    }

    /// <inheritdoc />
    public override bool IsValid(object? value, IValidationContext? validationContext) =>
        value switch
        {
            null => true,
            string text => !string.IsNullOrWhiteSpace(text) && ValidateDomain(text),
            _ => false
        };

    /// <summary>
    ///     验证域名有效性
    /// </summary>
    /// <param name="domain">域名</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal bool ValidateDomain(string domain)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);

        try
        {
            // 将 Unicode 域名转换为 Punycode 格式
            var asciiDomain = _idnMapping.GetAscii(domain);

            // 检查域名总长度是否超过 RFC 1034 规定的 253 字节限制
            // 参考文献：https://www.rfc-editor.org/info/rfc1034
            return asciiDomain.Length <= 253 && Regex().IsMatch(asciiDomain);
        }
        // 转换失败（如包含非法字符、未分配 Unicode 等），视为无效域名
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     域名正则表达式
    /// </summary>
    /// <returns>
    ///     <see cref="System.Text.RegularExpressions.Regex" />
    /// </returns>
    [GeneratedRegex(@"^([a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$")]
    private static partial Regex Regex();
}