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
///     组合验证器
/// </summary>
/// <typeparam name="T">对象类型</typeparam>
public class CompositeValidator<T> : ValidatorBase<T>, IValidatorInitializer, IDisposable
{
    /// <summary>
    ///     <see cref="ValidatorBase" /> 集合
    /// </summary>
    internal readonly IReadOnlyList<ValidatorBase> _validators;

    /// <summary>
    ///     <inheritdoc cref="CompositeValidator{T}" />
    /// </summary>
    /// <param name="validators">验证器列表</param>
    /// <param name="mode"><see cref="CompositeMode" />，默认值为：<see cref="CompositeMode.FailFast" /></param>
    public CompositeValidator(ValidatorBase[] validators, CompositeMode mode = CompositeMode.FailFast)
        : this(u => u.AddValidators(validators), mode)
    {
    }

    /// <summary>
    ///     <inheritdoc cref="CompositeValidator{T}" />
    /// </summary>
    /// <param name="configure">验证器配置委托</param>
    /// <param name="mode"><see cref="CompositeMode" />，默认值为：<see cref="CompositeMode.FailFast" /></param>
    public CompositeValidator(Action<FluentValidatorBuilder<T>> configure, CompositeMode mode = CompositeMode.FailFast)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        _validators = new FluentValidatorBuilder<T>().Build(configure);
        Mode = mode;

        ErrorMessageResourceAccessor = () => null!;
    }

    /// <summary>
    ///     <inheritdoc cref="CompositeMode" />
    /// </summary>
    /// <remarks>默认值为：<see cref="CompositeMode.FailFast" />。</remarks>
    public CompositeMode Mode { get; set; }

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
    public override bool IsValid(T? instance, ValidationContext<T> validationContext) =>
        Mode switch
        {
            CompositeMode.FailFast or CompositeMode.All => _validators.All(u => u.IsValid(instance, validationContext)),
            CompositeMode.Any => _validators.Any(u => u.IsValid(instance, validationContext)),
            _ => throw new NotSupportedException()
        };

    /// <inheritdoc />
    public override List<ValidationResult>? GetValidationResults(T? instance, ValidationContext<T> validationContext)
    {
        // 初始化验证结果列表
        var validationResults = new List<ValidationResult>();

        // 遍历验证器列表
        foreach (var validator in _validators)
        {
            // 获取对象验证结果列表
            if (validator.GetValidationResults(instance, validationContext) is { Count: > 0 } results)
            {
                // 追加验证结果列表
                validationResults.AddRange(results);

                // 检查验证器模式是否是遇到首个验证失败即停止后续验证
                if (Mode is CompositeMode.FailFast)
                {
                    break;
                }
            }
            // 检查验证器模式是否是任一验证器验证成功，即视为整体验证通过
            else if (Mode is CompositeMode.Any)
            {
                // 清空验证结果列表
                validationResults.Clear();

                break;
            }
        }

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
        // 初始化首个验证无效的验证器
        ValidatorBase? firstFailedValidator = null;

        // 遍历验证器列表
        foreach (var validator in _validators)
        {
            // 检查对象是否合法
            if (!validator.IsValid(instance, validationContext))
            {
                // 缓存首个验证无效的验证器
                firstFailedValidator ??= validator;

                // 检查验证器模式是否是遇到首个验证失败即停止后续验证
                if (Mode is CompositeMode.FailFast or CompositeMode.All)
                {
                    ThrowValidationException(instance, validator, validationContext);
                }
            }
            // 检查验证器模式是否是任一验证器验证成功，即视为整体验证通过
            else if (Mode is CompositeMode.Any)
            {
                return;
            }
        }

        // 空检查
        if (firstFailedValidator is not null)
        {
            ThrowValidationException(instance, firstFailedValidator, validationContext);
        }
    }

    /// <summary>
    ///     设置验证模式
    /// </summary>
    /// <param name="mode">
    ///     <see cref="CompositeMode" />
    /// </param>
    /// <returns>
    ///     <see cref="CompositeValidator{T}" />
    /// </returns>
    public CompositeValidator<T> UseMode(CompositeMode mode)
    {
        Mode = mode;

        return this;
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
    internal void ThrowValidationException(T? instance, ValidatorBase validator, ValidationContext<T> validationContext)
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

        // 释放所有验证器资源
        foreach (var validator in _validators)
        {
            if (validator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    /// <inheritdoc cref="IValidatorInitializer.InitializeServiceProvider" />
    internal void InitializeServiceProvider(Func<Type, object?>? serviceProvider)
    {
        // 遍历所有验证器并尝试同步 IServiceProvider 委托
        foreach (var validator in _validators)
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