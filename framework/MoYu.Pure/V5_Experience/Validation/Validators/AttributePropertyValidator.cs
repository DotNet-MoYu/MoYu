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

using MoYu.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace MoYu.Validation;

/// <summary>
///     属性验证特性验证器
/// </summary>
/// <typeparam name="T">对象类型</typeparam>
public class AttributePropertyValidator<T> : ValidatorBase<T>
{
    /// <summary>
    ///     属性值访问器
    /// </summary>
    internal readonly Func<T, object?> _getter;

    /// <summary>
    ///     <inheritdoc cref="AttributePropertyValidator{T}" />
    /// </summary>
    /// <param name="selector">属性选择器</param>
    public AttributePropertyValidator(Expression<Func<T, object?>> selector)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(selector);

        Property = selector.GetProperty();
        _getter = selector.Compile();

        ErrorMessageResourceAccessor = () => null!;
    }

    /// <summary>
    ///     <inheritdoc cref="PropertyInfo" />
    /// </summary>
    public PropertyInfo Property { get; }

    /// <inheritdoc />
    public override bool IsValid(T? instance, ValidationContext<T> validationContext)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(instance);

        // 获取显示名称
        var displayName = GetDisplayName(validationContext.DisplayName);

        return Validator.TryValidateProperty(GetValue(instance),
            CreateValidationContext(instance, displayName, validationContext), null);
    }

    /// <inheritdoc />
    public override List<ValidationResult>? GetValidationResults(T? instance, ValidationContext<T> validationContext)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(instance);

        // 获取显示名称
        var displayName = GetDisplayName(validationContext.DisplayName);

        // 初始化验证结果列表
        var validationResults = new List<ValidationResult>();

        Validator.TryValidateProperty(GetValue(instance),
            CreateValidationContext(instance, displayName, validationContext), validationResults);

        // 如果验证未通过且配置了自定义错误信息，则在首部添加自定义错误信息
        if (validationResults.Count > 0 && (string?)ErrorMessageString is not null)
        {
            validationResults.Insert(0,
                new ValidationResult(FormatErrorMessage(displayName),
                    validationContext.MemberNames ?? [Property.Name]));
        }

        return validationResults.ToResults();
    }

    /// <inheritdoc />
    public override void Validate(T? instance, ValidationContext<T> validationContext)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(instance);

        // 获取显示名称
        var displayName = GetDisplayName(validationContext.DisplayName);

        try
        {
            Validator.ValidateProperty(GetValue(instance),
                CreateValidationContext(instance, displayName, validationContext));
        }
        // 如果验证未通过且配置了自定义错误信息，则重新抛出异常
        catch (ValidationException e) when (ErrorMessageString is not null)
        {
            throw new ValidationException(
                new ValidationResult(FormatErrorMessage(displayName),
                    validationContext.MemberNames ?? e.ValidationResult.MemberNames), e.ValidationAttribute, e.Value)
            {
                Source = e.Source
            };
        }
    }

    /// <summary>
    ///     获取属性值
    /// </summary>
    /// <param name="instance">对象</param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    public object? GetValue(T instance)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(instance);

        return _getter(instance);
    }

    /// <summary>
    ///     获取显示名称
    /// </summary>
    /// <param name="name">显示名称</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public string GetDisplayName(string? name) =>
        name ?? Property.GetCustomAttribute<DisplayAttribute>(true)?.GetName() ??
        Property.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName ?? GetMemberName();

    /// <summary>
    ///     获取成员名称
    /// </summary>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public string GetMemberName() => Property.Name;

    /// <summary>
    ///     创建 <see cref="ValidationContext" /> 实例
    /// </summary>
    /// <param name="instance">对象</param>
    /// <param name="name">显示名称</param>
    /// <param name="context">
    ///     <see cref="ValidationContext{T}" />
    /// </param>
    /// <returns>
    ///     <see cref="ValidationContext" />
    /// </returns>
    internal ValidationContext CreateValidationContext(object instance, string? name, ValidationContext<T> context)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(instance);

        // 初始化 ValidationContext 实例
        var validationContext = new ValidationContext(instance, context, context.Items) { MemberName = Property.Name };

        // 空检查
        if (name is not null)
        {
            validationContext.DisplayName = name;
        }

        // 空检查
        if (context.RuleSets is not null)
        {
            // 设置规则集
            validationContext.WithRuleSets(context.RuleSets);
        }

        return validationContext;
    }
}