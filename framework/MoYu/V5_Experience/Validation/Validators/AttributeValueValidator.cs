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
///     单值验证特性验证器
/// </summary>
public class AttributeValueValidator : ValidatorBase, IDisposable
{
    /// <summary>
    ///     用于值验证的 <see cref="ValidationContext" /> 占位对象
    /// </summary>
    internal static readonly object _sentinel = new();

    /// <summary>
    ///     需要监听属性变更的属性名集合
    /// </summary>
    internal readonly string[] _observedPropertyNames =
    [
        nameof(ErrorMessage), nameof(ErrorMessageResourceType), nameof(ErrorMessageResourceName)
    ];

    /// <summary>
    ///     <inheritdoc cref="AttributeValueValidator" />
    /// </summary>
    /// <param name="attributes">验证特性列表</param>
    public AttributeValueValidator(params ValidationAttribute[] attributes)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(attributes);

        // 确保数组元素不存在 null 值
        if (attributes.Any(u => (ValidationAttribute?)u is null))
        {
            // ReSharper disable once LocalizableElement
            throw new ArgumentException("Attributes cannot contain null elements.", nameof(attributes));
        }

        Attributes = attributes;

        // 订阅属性变更事件
        PropertyChanged += OnPropertyChanged;

        ErrorMessageResourceAccessor = () => null!;
    }

    /// <summary>
    ///     验证特性列表
    /// </summary>
    public ValidationAttribute[] Attributes { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public override bool IsValid(object? value, IValidationContext? validationContext) =>
        Validator.TryValidateValue(value, CreateValidationContext(value, validationContext), null, Attributes);

    /// <inheritdoc />
    public override List<ValidationResult>? GetValidationResults(object? value, IValidationContext? validationContext)
    {
        // 初始化验证结果列表和成员名称列表
        var validationResults = new List<ValidationResult>();

        Validator.TryValidateValue(value, CreateValidationContext(value, validationContext), validationResults,
            Attributes);

        // 如果验证未通过且配置了自定义错误信息，则在首部添加自定义错误信息
        // 注意：当验证特性列表有且只有一个时，跳过以下操作
        if (validationResults.Count > 0 && Attributes.Length != 1 && (string?)ErrorMessageString is not null)
        {
            validationResults.Insert(0,
                new ValidationResult(FormatErrorMessage(validationContext?.DisplayName!),
                    validationContext?.MemberNames));
        }

        return validationResults.ToResults();
    }

    /// <inheritdoc />
    public override void Validate(object? value, IValidationContext? validationContext)
    {
        try
        {
            Validator.ValidateValue(value, CreateValidationContext(value, validationContext), Attributes);
        }
        // 如果验证未通过且配置了自定义错误信息，则重新抛出异常
        // 注意：当验证特性列表有且只有一个时，跳过以下操作
        catch (ValidationException e) when (Attributes.Length != 1 && ErrorMessageString is not null)
        {
            throw new ValidationException(
                new ValidationResult(FormatErrorMessage(validationContext?.DisplayName!),
                    validationContext?.MemberNames),
                e.ValidationAttribute, e.Value) { Source = e.Source };
        }
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    /// <param name="disposing">是否释放托管资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 移除属性变更事件
            PropertyChanged -= OnPropertyChanged;
        }
    }

    /// <summary>
    ///     订阅属性变更事件
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="eventArgs">
    ///     <see cref="ValidationPropertyChangedEventArgs" />
    /// </param>
    internal void OnPropertyChanged(object? sender, ValidationPropertyChangedEventArgs eventArgs)
    {
        // 注意：当验证特性列表存在多个时，跳过以下操作
        if (Attributes.Length != 1)
        {
            return;
        }

        // 检查是否是需要同步的属性名
        if (!_observedPropertyNames.Contains(eventArgs.PropertyName))
        {
            return;
        }

        // 获取单个验证特性
        var attribute = Attributes.Single();

        // 应用属性变更到验证特性对应的属性中
        attribute.GetType().GetProperty(eventArgs.PropertyName!)?.SetValue(attribute, eventArgs.PropertyValue);
    }

    /// <summary>
    ///     创建 <see cref="ValidationContext" /> 实例
    /// </summary>
    /// <param name="value">对象</param>
    /// <param name="context">
    ///     <see cref="IValidationContext" />
    /// </param>
    /// <returns>
    ///     <see cref="ValidationContext" />
    /// </returns>
    internal static ValidationContext CreateValidationContext(object? value, IValidationContext? context)
    {
        // 初始化 ValidationContext 实例
        var validationContext =
            new ValidationContext(value ?? _sentinel, context, context?.Items)
            {
                MemberName = context?.MemberNames?.FirstOrDefault()
            };

        // 空检查
        if (context?.DisplayName is not null)
        {
            validationContext.DisplayName = context.DisplayName;
        }

        // 空检查
        if (context?.RuleSets is not null)
        {
            // 设置规则集
            validationContext.WithRuleSets(context.RuleSets);
        }

        return validationContext;
    }
}