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

using MoYu.Validation;
using MoYu.Validation.Resources;

namespace System.ComponentModel.DataAnnotations;

/// <summary>
///     MD5 字符串验证特性
/// </summary>
/// <remarks>支持 32 位标准格式，可选 16 字符截断格式。</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class MD5StringAttribute : ValidationBaseAttribute
{
    /// <inheritdoc cref="MD5StringValidator" />
    internal readonly MD5StringValidator _validator;

    /// <summary>
    ///     <inheritdoc cref="MD5StringAttribute" />
    /// </summary>
    public MD5StringAttribute()
    {
        _validator = new MD5StringValidator();

        UseResourceKey(() => nameof(ValidationMessages.MD5StringValidator_ValidationError));
    }

    /// <summary>
    ///     是否允许截断的 128 位哈希值（16 字节的十六进制字符串，共 32 字符）
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    public bool AllowShortFormat
    {
        get;
        set
        {
            field = value;
            _validator.AllowShortFormat = value;
        }
    }

    /// <inheritdoc />
    public override bool IsValid(object? value) => _validator.IsValid(value);
}