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

namespace MoYu.Validation;

/// <summary>
///     URL 地址验证器
/// </summary>
public class UrlValidator : ValidatorBase
{
    /// <summary>
    ///     <inheritdoc cref="UrlValidator" />
    /// </summary>
    public UrlValidator() => UseResourceKey(GetResourceKey);

    /// <summary>
    ///     是否支持 FTP 协议
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    public bool SupportsFtp { get; set; }

    /// <inheritdoc />
    public override bool IsValid(object? value, IValidationContext? validationContext) =>
        value switch
        {
            null => true,
            string text => !string.IsNullOrWhiteSpace(text) && ValidateUrl(text),
            _ => false
        };

    /// <summary>
    ///     验证 URL 地址有效性
    /// </summary>
    /// <param name="url">URL 地址</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal bool ValidateUrl(string url)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        // 尝试创建一个新的 Uri 实例
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
        {
            return false;
        }

        return (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps ||
                (SupportsFtp && uriResult.Scheme == Uri.UriSchemeFtp)) &&
               !string.IsNullOrEmpty(uriResult.Host); // 不接受无主机名的 URL（如：http:///path）
    }

    /// <summary>
    ///     获取错误信息对应的资源键
    /// </summary>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal string GetResourceKey() =>
        SupportsFtp
            ? nameof(ValidationMessages.UrlValidator_ValidationError_SupportsFtp)
            : nameof(ValidationMessages.UrlValidator_ValidationError);
}