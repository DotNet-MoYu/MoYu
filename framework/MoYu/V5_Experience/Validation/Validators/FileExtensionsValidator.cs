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
///     文件扩展名验证器
/// </summary>
public class FileExtensionsValidator : ValidatorBase
{
    /// <summary>
    ///     <inheritdoc cref="FileExtensionsValidator" />
    /// </summary>
    /// <remarks>默认文件扩展名为：<c>png,jpg,jpeg,gif</c>。</remarks>
    public FileExtensionsValidator() =>
        UseResourceKey(() => nameof(ValidationMessages.FileExtensionsValidator_ValidationError));

    /// <summary>
    ///     文件扩展名
    /// </summary>
    public string Extensions
    {
        get => string.IsNullOrWhiteSpace(field) ? "png,jpg,jpeg,gif" : field;
        set;
    }

    /// <summary>
    ///     格式化后的文件扩展名列表
    /// </summary>
    internal string ExtensionsFormatted => ExtensionsParsed.Aggregate((left, right) => left + ", " + right);

    /// <summary>
    ///     标准化后的文件扩展名字符串
    /// </summary>
    internal string ExtensionsNormalized =>
        Extensions.Replace(" ", string.Empty).Replace(".", string.Empty).ToLowerInvariant();

    /// <summary>
    ///     解析后的文件扩展名集合
    /// </summary>
    internal IEnumerable<string> ExtensionsParsed => ExtensionsNormalized.Split(',').Select(e => "." + e);

    /// <inheritdoc />
    public override bool IsValid(object? value, IValidationContext? validationContext) =>
        value is null || (value is string valueAsString && ValidateExtension(valueAsString));

    /// <inheritdoc />
    public override string FormatErrorMessage(string name) =>
        string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, ExtensionsFormatted);

    /// <summary>
    ///     验证文件扩展名是否在允许的列表中
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal bool ValidateExtension(string fileName) =>
        ExtensionsParsed.Contains(Path.GetExtension(fileName).ToLowerInvariant());
}