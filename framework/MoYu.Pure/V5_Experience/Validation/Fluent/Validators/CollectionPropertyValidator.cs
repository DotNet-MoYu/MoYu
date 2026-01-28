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

using System.Linq.Expressions;

namespace MoYu.Validation;

/// <summary>
///     集合类型属性验证器
/// </summary>
/// <typeparam name="T">对象类型</typeparam>
/// <typeparam name="TElement">元素类型</typeparam>
public sealed class
    CollectionPropertyValidator<T, TElement> : PropertyValidator<T, IEnumerable<TElement>?,
    CollectionPropertyValidator<T, TElement>>
{
    /// <summary>
    ///     元素过滤委托
    /// </summary>
    internal Func<TElement, bool>? _elementFilter;

    /// <inheritdoc />
    internal CollectionPropertyValidator(Expression<Func<T, IEnumerable<TElement?>?>> selector,
        ObjectValidator<T> objectValidator) : base(selector!, objectValidator)
    {
    }

    /// <summary>
    ///     筛选用于执行验证的集合元素
    /// </summary>
    /// <param name="elementFilter">元素过滤委托</param>
    /// <returns>
    ///     <see cref="CollectionPropertyValidator{T,TElement}" />
    /// </returns>
    public CollectionPropertyValidator<T, TElement> Where(Func<TElement, bool>? elementFilter)
    {
        _elementFilter = elementFilter;

        // 同步元素过滤委托
        Validators.OfType<CollectionValidator<TElement>>().FirstOrDefault()?.Where(elementFilter);

        return this;
    }

    /// <summary>
    ///     设置集合元素对象验证器
    /// </summary>
    /// <param name="validatorFactory">
    ///     <see cref="ObjectValidator{T}" /> 工厂委托
    /// </param>
    /// <returns>
    ///     <see cref="CollectionPropertyValidator{T,TElement}" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public CollectionPropertyValidator<T, TElement> SetValidator(
        Func<string?[]?, IDictionary<object, object?>?, ValidatorOptions, ObjectValidator<TElement>?> validatorFactory)
    {
        // 确保尚未为集合元素分配验证器
        EnsureNoElementValidatorAssigned();

        // 空检查
        ArgumentNullException.ThrowIfNull(validatorFactory);

        // 检查集合元素是否是类类型
        if (!typeof(TElement).IsClass || typeof(TElement) == typeof(string))
        {
            throw new InvalidOperationException(
                $"Collection element type '{typeof(TElement)}' is not a reference type. `{nameof(SetValidator)}` (object validator) and `{nameof(ChildRules)}` are only supported for class types. For value types, use `{nameof(EachRules)}` or `{nameof(SetValidator)}` (value validator) instead.");
        }

        // 调用工厂方法，传入当前 RuleSets、Items 和 Options
        var objectValidator = validatorFactory(RuleSets, _objectValidator.Items, _objectValidator.Options);

        // 空检查
        if (objectValidator is null)
        {
            return this;
        }

        // 继承当前规则集
        objectValidator.SetInheritedRuleSetsIfNotSet(RuleSets);

        // 同步 IServiceProvider 委托
        objectValidator.InitializeServiceProvider(_serviceProvider);

        return AddValidator(
            new CollectionValidator<TElement>(objectValidator) { IsNested = true /*标记为嵌套验证器*/ }.Where(_elementFilter));
    }

    /// <summary>
    ///     设置集合元素对象验证器
    /// </summary>
    /// <param name="validator">
    ///     <see cref="ObjectValidator{T}" />
    /// </param>
    /// <returns>
    ///     <see cref="CollectionPropertyValidator{T,TElement}" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public CollectionPropertyValidator<T, TElement> SetValidator(ObjectValidator<TElement>? validator) =>
        SetValidator((_, _, _) => validator);

    /// <summary>
    ///     为集合元素继续配置规则
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="CollectionPropertyValidator{T,TElement}" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public CollectionPropertyValidator<T, TElement> ChildRules(Action<ObjectValidator<TElement>> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        return SetValidator((ruleSets, items, options) =>
        {
            // 初始化集合元素对象验证器实例
            var elementValidator =
                new ObjectValidator<TElement>(items) { InheritedRuleSets = ruleSets }.ConfigureOptions(options);

            // 调用自定义配置委托
            configure(elementValidator);

            return elementValidator;
        });
    }

    /// <summary>
    ///     设置集合元素值验证器
    /// </summary>
    /// <param name="validatorFactory">
    ///     <see cref="ValueValidator{T}" /> 工厂委托
    /// </param>
    /// <returns>
    ///     <see cref="CollectionPropertyValidator{T,TElement}" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public CollectionPropertyValidator<T, TElement> SetValidator(
        Func<IDictionary<object, object?>?, ValueValidator<TElement>?> validatorFactory)
    {
        // 确保尚未为集合元素分配验证器
        EnsureNoElementValidatorAssigned();

        // 空检查
        ArgumentNullException.ThrowIfNull(validatorFactory);

        // 调用工厂方法，传入当前 Items 
        var valueValidator = validatorFactory(_objectValidator.Items);

        // 空检查
        if (valueValidator is null)
        {
            return this;
        }

        // 同步 IServiceProvider 委托
        valueValidator.InitializeServiceProvider(_serviceProvider);

        return AddValidator(
            new CollectionValidator<TElement>(valueValidator) { IsNested = true /*标记为嵌套验证器*/ }.Where(_elementFilter));
    }

    /// <summary>
    ///     设置集合元素值验证器
    /// </summary>
    /// <param name="validator">
    ///     <see cref="ValueValidator{T}" />
    /// </param>
    /// <returns>
    ///     <see cref="CollectionPropertyValidator{T,TElement}" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public CollectionPropertyValidator<T, TElement> SetValidator(ValueValidator<TElement>? validator) =>
        SetValidator(_ => validator);

    /// <summary>
    ///     为集合元素继续配置规则
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="CollectionPropertyValidator{T,TElement}" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public CollectionPropertyValidator<T, TElement> EachRules(Action<ValueValidator<TElement>> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        return SetValidator(items =>
        {
            // 初始化集合元素值验证器实例
            var valueValidator = new ValueValidator<TElement>(items);

            // 调用自定义配置委托
            configure(valueValidator);

            return valueValidator;
        });
    }

    /// <summary>
    ///     确保尚未为集合元素分配验证器
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    internal void EnsureNoElementValidatorAssigned()
    {
        if (Validators.OfType<CollectionValidator<TElement>>().Any())
        {
            throw new InvalidOperationException(
                $"An element validator has already been assigned. Only one is allowed per collection element. To configure nested rules, use `{nameof(ChildRules)}` or `{nameof(EachRules)}` within a single validator.");
        }
    }
}