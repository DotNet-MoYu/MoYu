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
///     用户名验证特性
/// </summary>
/// <remarks>
///     长度 4-16 位，以字母开头，支持字母、数字、下划线、减号组合。
///     不允许包含空格或其他特殊字符，禁止连续特殊字符（如 __）。
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class UserNameAttribute : ValidationBaseAttribute
{
    /// <inheritdoc cref="UserNameValidator" />
    internal readonly UserNameValidator _validator;

    /// <summary>
    ///     <inheritdoc cref="UserNameAttribute" />
    /// </summary>
    public UserNameAttribute()
    {
        _validator = new UserNameValidator();

        UseResourceKey(() => nameof(ValidationMessages.UserNameValidator_ValidationError));
    }

    /// <inheritdoc />
    public override bool IsValid(object? value) => _validator.IsValid(value);
}