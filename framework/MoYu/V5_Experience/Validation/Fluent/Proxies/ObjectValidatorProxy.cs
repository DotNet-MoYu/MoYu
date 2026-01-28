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
///     对象验证器代理
/// </summary>
/// <typeparam name="T">对象类型</typeparam>
public class ObjectValidatorProxy<T> : ValidatorBase<T>, IValidatorInitializer, IMemberPathRepairable,
    IDisposable
{
    /// <inheritdoc cref="IObjectValidator{T}" />
    internal readonly IObjectValidator<T> _objectValidator;

    /// <summary>
    ///     <inheritdoc cref="ObjectValidatorProxy{T}" />
    /// </summary>
    /// <param name="objectValidator">
    ///     <see cref="IObjectValidator{T}" />
    /// </param>
    public ObjectValidatorProxy(IObjectValidator<T> objectValidator)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(objectValidator);

        _objectValidator = objectValidator;

        ErrorMessageResourceAccessor = () => null!;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    string? IMemberPathRepairable.MemberPath { get; set; }

    /// <inheritdoc />
    void IMemberPathRepairable.RepairMemberPaths(string? memberPath) => RepairMemberPaths(memberPath);

    /// <inheritdoc />
    void IValidatorInitializer.InitializeServiceProvider(Func<Type, object?>? serviceProvider) =>
        InitializeServiceProvider(serviceProvider);

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

        _objectValidator.Dispose();
    }

    /// <inheritdoc />
    public override bool IsValid(T? instance, ValidationContext<T> validationContext) =>
        _objectValidator.IsValid(instance, validationContext.RuleSets);

    /// <inheritdoc />
    public override List<ValidationResult>? GetValidationResults(T? instance, ValidationContext<T> validationContext) =>
        _objectValidator.GetValidationResults(instance, validationContext.RuleSets);

    /// <inheritdoc />
    public override void Validate(T? instance, ValidationContext<T> validationContext) =>
        _objectValidator.Validate(instance, validationContext.RuleSets);

    /// <inheritdoc cref="IValidatorInitializer.InitializeServiceProvider" />
    internal void InitializeServiceProvider(Func<Type, object?>? serviceProvider) =>
        _objectValidator.InitializeServiceProvider(serviceProvider);

    /// <inheritdoc cref="IMemberPathRepairable.RepairMemberPaths" />
    internal virtual void RepairMemberPaths(string? memberPath)
    {
        if (_objectValidator is IMemberPathRepairable memberPathRepairable)
        {
            memberPathRepairable.RepairMemberPaths(memberPath);
        }
    }
}