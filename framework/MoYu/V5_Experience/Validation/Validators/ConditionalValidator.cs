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

using System.ComponentModel.DataAnnotations;

namespace MoYu.Validation;

/// <summary>
///     条件验证器
/// </summary>
/// <typeparam name="T">对象类型</typeparam>
public class ConditionalValidator<T> : ValidatorBase<T>, IValidatorInitializer, IDisposable
{
    /// <summary>
    ///     <see cref="ConditionResult{T}" /> 构建结果
    /// </summary>
    internal readonly ConditionResult<T> _conditionResult;

    /// <summary>
    ///     <inheritdoc cref="ConditionalValidator{T}" />
    /// </summary>
    /// <param name="buildConditions">条件验证构建器配置委托</param>
    public ConditionalValidator(Action<ConditionBuilder<T>> buildConditions)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(buildConditions);

        _conditionResult = new ConditionBuilder<T>().Build(buildConditions);

        ErrorMessageResourceAccessor = () => null!;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    void IValidatorInitializer.InitializeServiceProvider(Func<Type, object?>? serviceProvider) =>
        InitializeServiceProvider(serviceProvider);

    /// <inheritdoc />
    public override bool IsValid(T? instance, ValidationContext<T> validationContext)
    {
        // 获取匹配到的验证器列表
        var matchedValidators = GetMatchedValidators(instance);

        return matchedValidators is null or { Count: 0 } ||
               matchedValidators.All(u => u.IsValid(instance, validationContext));
    }

    /// <inheritdoc />
    public override List<ValidationResult>? GetValidationResults(T? instance, ValidationContext<T> validationContext)
    {
        // 获取匹配到的验证器列表和成员名称列表
        var matchedValidators = GetMatchedValidators(instance);

        // 空检查
        if (matchedValidators is null or { Count: 0 })
        {
            return null;
        }

        // 获取验证结果列表
        var validationResults = matchedValidators
            .SelectMany(u => u.GetValidationResults(instance, validationContext) ?? []).ToList();

        // 如果验证未通过且配置了自定义错误信息，则在首部添加自定义错误信息
        if (validationResults.Count > 0 && (string?)ErrorMessageString is not null)
        {
            validationResults.Insert(0,
                new ValidationResult(FormatErrorMessage(validationContext.DisplayName), validationContext.MemberNames));
        }

        return validationResults.ToResults();
    }

    /// <inheritdoc />
    public override void Validate(T? instance, ValidationContext<T> validationContext)
    {
        // 获取匹配到的验证器列表
        var matchedValidators = GetMatchedValidators(instance);

        // 空检查
        if (matchedValidators is null or { Count: 0 })
        {
            return;
        }

        // 遍历验证器列表
        foreach (var validator in matchedValidators)
        {
            // 检查对象是否合法
            if (!validator.IsValid(instance, validationContext))
            {
                ThrowValidationException(instance, validator, validationContext);
            }
        }
    }

    /// <summary>
    ///     抛出验证异常
    /// </summary>
    /// <param name="instance">对象</param>
    /// <param name="validator">
    ///     <see cref="ValidatorBase" />
    /// </param>
    /// <param name="validationContext">
    ///     <see cref="ValidationContext{T}" />
    /// </param>
    /// <exception cref="ValidationException"></exception>
    internal void ThrowValidationException(T? instance, ValidatorBase validator,
        ValidationContext<T> validationContext)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(validator);

        // 检查是否配置了自定义错误信息
        if ((string?)ErrorMessageString is null)
        {
            validator.Validate(instance, validationContext);
        }
        else
        {
            throw new ValidationException(
                new ValidationResult(FormatErrorMessage(validationContext.DisplayName), validationContext.MemberNames),
                null, instance);
        }
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    /// <param name="disposing">是否释放托管资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        // 获取所有验证器列表
        var validators =
            (_conditionResult.DefaultRules ?? []).Concat(
                _conditionResult.ConditionalRules.SelectMany(u => u.Validators));

        // 释放所有验证器资源
        foreach (var validator in validators)
        {
            if (validator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>
    ///     获取匹配到的验证器列表
    /// </summary>
    /// <param name="instance">对象</param>
    /// <returns>
    ///     <see cref="IReadOnlyList{T}" />
    /// </returns>
    internal IReadOnlyList<ValidatorBase>? GetMatchedValidators(T? instance)
    {
        // 初始化匹配到的验证器列表
        IReadOnlyList<ValidatorBase>? matchedValidators = null;

        // 遍历并查找第一个条件匹配的验证器列表
        foreach (var (condition, validators) in _conditionResult.ConditionalRules)
        {
            // ReSharper disable once InvertIf
            if (condition(instance!))
            {
                matchedValidators = validators;
                break;
            }
        }

        // 没有匹配条件时使用默认验证器列表
        matchedValidators ??= _conditionResult.DefaultRules;

        return matchedValidators;
    }

    /// <inheritdoc cref="IValidatorInitializer.InitializeServiceProvider" />
    internal void InitializeServiceProvider(Func<Type, object?>? serviceProvider)
    {
        // 获取所有验证器列表
        var validators =
            (_conditionResult.DefaultRules ?? []).Concat(
                _conditionResult.ConditionalRules.SelectMany(u => u.Validators));

        // 遍历所有验证器并尝试同步 IServiceProvider 委托
        foreach (var validator in validators)
        {
            // 检查验证器是否实现 IValidatorInitializer 接口
            if (validator is IValidatorInitializer initializer)
            {
                // 同步 IServiceProvider 委托
                initializer.InitializeServiceProvider(serviceProvider);
            }
        }
    }
}