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
using MoYu.Validation.Resources;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace MoYu.Validation;

/// <summary>
///     比较两个属性验证器
/// </summary>
/// <typeparam name="T">对象类型</typeparam>
public class CompareValidator<T> : ValidatorBase<T>
{
    /// <summary>
    ///     属性值访问器
    /// </summary>
    internal readonly Func<T, object?> _otherPropertyGetter;

    /// <summary>
    ///     其他属性值访问器
    /// </summary>
    internal readonly Func<T, object?> _propertyGetter;

    /// <summary>
    ///     <inheritdoc cref="CompareValidator{T}" />
    /// </summary>
    /// <param name="propertySelector">属性选择器</param>
    /// <param name="otherPropertyName">其他属性的名称</param>
    public CompareValidator(Expression<Func<T, object?>> propertySelector, string otherPropertyName)
        : this(propertySelector, CreatePropertySelector(otherPropertyName))
    {
    }

    /// <summary>
    ///     <inheritdoc cref="CompareValidator{T}" />
    /// </summary>
    /// <param name="propertySelector">属性选择器</param>
    /// <param name="otherPropertySelector">其他属性选择器</param>
    public CompareValidator(Expression<Func<T, object?>> propertySelector,
        Expression<Func<T, object?>> otherPropertySelector)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(propertySelector);
        ArgumentNullException.ThrowIfNull(otherPropertySelector);

        Property = propertySelector.GetProperty();
        _propertyGetter = propertySelector.Compile();

        OtherProperty = otherPropertySelector.GetProperty();
        _otherPropertyGetter = otherPropertySelector.Compile();

        UseResourceKey(() => nameof(ValidationMessages.CompareValidator_ValidationError));
    }

    /// <summary>
    ///     <inheritdoc cref="PropertyInfo" />
    /// </summary>
    public PropertyInfo Property { get; }

    /// <summary>
    ///     <inheritdoc cref="PropertyInfo" />
    /// </summary>
    public PropertyInfo OtherProperty { get; }

    /// <inheritdoc />
    public override bool IsValid(T? instance, ValidationContext<T> validationContext)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(instance);

        return Equals(_propertyGetter(instance), _otherPropertyGetter(instance));
    }

    /// <inheritdoc />
    public override string? FormatErrorMessage(string name) =>
        string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, GetDisplayNameForProperty(OtherProperty));

    /// <summary>
    ///     获取属性显示名称
    /// </summary>
    /// <param name="property">
    ///     <see cref="PropertyInfo" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string GetDisplayNameForProperty(PropertyInfo property)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(property);

        return property.GetCustomAttribute<DisplayAttribute>(true)?.GetName() ??
               property.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName ?? property.Name;
    }

    /// <summary>
    ///     为指定属性创建属性选择器表达式
    /// </summary>
    /// <param name="propertyName">属性名</param>
    /// <returns>
    ///     <see cref="Expression{TDelegate}" />
    /// </returns>
    internal static Expression<Func<T, object?>> CreatePropertySelector(string propertyName)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // 创建 Lambda 表达式的参数
        var parameter = Expression.Parameter(typeof(T), "u");

        // 尝试从类型 T 中获取指定属性并将其值转换为 object?
        var converted = Expression.Convert(Expression.Property(parameter, propertyName), typeof(object));

        // 构建 Lambda 表达式：u => (object?)u.PropertyName
        return Expression.Lambda<Func<T, object?>>(converted, parameter);
    }
}