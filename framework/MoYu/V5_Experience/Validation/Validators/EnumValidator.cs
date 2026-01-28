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
///     <inheritdoc cref="EnumValidator" />
/// </summary>
/// <typeparam name="TEnum">枚举类型</typeparam>
public class EnumValidator<TEnum> : EnumValidator
    where TEnum : struct, Enum
{
    /// <inheritdoc />
    public EnumValidator() : base(typeof(TEnum))
    {
    }
}

/// <summary>
///     枚举验证器
/// </summary>
public class EnumValidator : ValidatorBase
{
    /// <summary>
    ///     <inheritdoc cref="EnumValidator" />
    /// </summary>
    /// <param name="enumType">枚举类型</param>
    /// <exception cref="ArgumentException"></exception>
    public EnumValidator(Type enumType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(enumType);

        // 检查是否是枚举
        if (!enumType.IsEnum)
        {
            // ReSharper disable once LocalizableElement
            throw new ArgumentException($"The type '{enumType.Name}' is not an enumeration type.", nameof(enumType));
        }

        EnumType = enumType;

        UseResourceKey(GetResourceKey);
    }

    /// <summary>
    ///     枚举类型
    /// </summary>
    public Type EnumType { get; }

    /// <summary>
    ///     是否支持 Flags 模式
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    public bool SupportFlags { get; set; }

    /// <inheritdoc />
    public override bool IsValid(object? value, IValidationContext? validationContext) =>
        value switch
        {
            null => true,
            Enum otherEnum when otherEnum.GetType() != EnumType => false,
            string stringValue when string.IsNullOrWhiteSpace(stringValue) => false,
            _ => IsEnumValueDefined(value)
        };

    /// <inheritdoc />
    public override string FormatErrorMessage(string name) =>
        string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, EnumType.Name);

    /// <summary>
    ///     判断值是否为合法的枚举成员
    /// </summary>
    /// <remarks>支持 Flags 模式。</remarks>
    /// <param name="value">值</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal bool IsEnumValueDefined(object value)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(value);

        // 检查是否是非 Flags 模式或枚举类型无 [Flags]
        if (!SupportFlags || !EnumType.IsDefined(typeof(FlagsAttribute), false))
        {
            return Enum.IsDefined(EnumType, value);
        }

        // 显式检查枚举
        if (Enum.IsDefined(EnumType, value))
        {
            return true;
        }

        try
        {
            // 检查 Flags 模式下是否所有位都属于已定义的枚举值
            var allDefinedValues = Enum.GetValues(EnumType).Cast<object>().Select(Convert.ToUInt64)
                .Aggregate((a, b) => a | b);

            var input = Convert.ToUInt64(value);
            return input != 0 && (input & ~allDefinedValues) == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     获取错误信息对应的资源键
    /// </summary>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal string GetResourceKey() =>
        SupportFlags
            ? nameof(ValidationMessages.EnumValidator_ValidationError_SupportFlags)
            : nameof(ValidationMessages.EnumValidator_ValidationError);
}