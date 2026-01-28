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
///     集合验证器
/// </summary>
/// <typeparam name="TElement">元素类型</typeparam>
public class CollectionValidator<TElement> : ValidatorBase<IEnumerable<TElement>>, ICollectionValidator<TElement>
{
    /// <inheritdoc cref="IObjectValidator{T}" />
    internal readonly IObjectValidator<TElement> _elementValidator;

    /// <inheritdoc cref="IMemberPathRepairable" />
    internal readonly IMemberPathRepairable? _memberPathRepairable;

    /// <summary>
    ///     元素过滤委托
    /// </summary>
    internal Func<TElement, bool>? _elementFilter;

    /// <summary>
    ///     对象图中的属性路径
    /// </summary>
    internal string? _memberPath;

    /// <summary>
    ///     <inheritdoc cref="CollectionValidator{TElement}" />
    /// </summary>
    /// <param name="elementValidator">
    ///     <see cref="IObjectValidator{T}" />
    /// </param>
    public CollectionValidator(IObjectValidator<TElement> elementValidator)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(elementValidator);

        _elementValidator = elementValidator;
        _memberPathRepairable = elementValidator as IMemberPathRepairable;

        ErrorMessageResourceAccessor = () => null!;
    }

    /// <summary>
    ///     指示当前验证器是否作为子对象验证器
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    internal bool IsNested { get; set; }

    /// <inheritdoc />
    string? IMemberPathRepairable.MemberPath
    {
        get => _memberPath;
        set => _memberPath = value;
    }

    /// <inheritdoc />
    void IMemberPathRepairable.RepairMemberPaths(string? memberPath) => RepairMemberPaths(memberPath);

    /// <inheritdoc />
    public bool IsValid(IEnumerable<TElement>? instance, string?[]? ruleSets = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(instance);

        return GetValidatingElements(instance).All(u => _elementValidator.IsValid(u, ruleSets));
    }

    /// <inheritdoc />
    public List<ValidationResult>? GetValidationResults(IEnumerable<TElement>? instance, string?[]? ruleSets = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(instance);

        // 初始化验证结果列表
        var validationResults = new List<ValidationResult>();

        // 遍历用于执行验证的集合元素
        var index = 0;
        foreach (var element in GetValidatingElements(instance))
        {
            // 获取原始属性路径
            var originalPath = _memberPathRepairable?.MemberPath;

            // 设置当前属性路径
            _memberPathRepairable?.MemberPath = $"{_memberPath}[{index}]";

            try
            {
                validationResults.AddRange(_elementValidator.GetValidationResults(element, ruleSets) ?? []);
            }
            finally
            {
                // 恢复原始属性路径
                _memberPathRepairable?.MemberPath = originalPath;
            }

            index++;
        }

        return validationResults.ToResults();
    }

    /// <inheritdoc />
    public void Validate(IEnumerable<TElement>? instance, string?[]? ruleSets = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(instance);

        // 遍历用于执行验证的集合元素
        var index = 0;
        foreach (var element in GetValidatingElements(instance))
        {
            // 获取原始属性路径
            var originalPath = _memberPathRepairable?.MemberPath;

            // 设置当前属性路径
            _memberPathRepairable?.MemberPath = $"{_memberPath}[{index}]";

            try
            {
                _elementValidator.Validate(element, ruleSets);
            }
            finally
            {
                // 恢复原始属性路径
                _memberPathRepairable?.MemberPath = originalPath;
            }

            index++;
        }
    }

    /// <inheritdoc />
    public virtual void InitializeServiceProvider(Func<Type, object?>? serviceProvider) =>
        _elementValidator.InitializeServiceProvider(serviceProvider);

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    bool IObjectValidator.IsValid(object? instance, string?[]? ruleSets) =>
        IsValid((IEnumerable<TElement>?)instance, ruleSets);

    /// <inheritdoc />
    List<ValidationResult>? IObjectValidator.GetValidationResults(object? instance, string?[]? ruleSets) =>
        GetValidationResults((IEnumerable<TElement>?)instance, ruleSets);

    /// <inheritdoc />
    void IObjectValidator.Validate(object? instance, string?[]? ruleSets) =>
        Validate((IEnumerable<TElement>?)instance, ruleSets);

    /// <inheritdoc />
    public List<ValidationResult> ToResults(ValidationContext validationContext, bool disposeAfterValidation = true)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(validationContext);

        // 同步 IServiceProvider 委托
        InitializeServiceProvider(validationContext.GetService);

        // 尝试从 ValidationContext.Items 中解析验证选项中的规则集
        string?[]? ruleSets = null;
        if (validationContext.Items.TryGetValue(Constants.ValidationOptionsKey, out var metadataObj) &&
            metadataObj is ValidationOptionsMetadata metadata)
        {
            ruleSets = metadata.RuleSets;
        }

        try
        {
            // 获取对象验证结果列表
            return GetValidationResults((IEnumerable<TElement>)validationContext.ObjectInstance, ruleSets) ?? [];
        }
        finally
        {
            // 自动释放资源
            if (disposeAfterValidation)
            {
                Dispose();
            }
        }
    }

    /// <inheritdoc />
    public override bool IsValid(IEnumerable<TElement>? instance,
        ValidationContext<IEnumerable<TElement>> validationContext)
    {
        // 检查是否是嵌套验证器
        if (instance is null && IsNested)
        {
            return true;
        }

        return IsValid(instance, validationContext.RuleSets);
    }

    /// <inheritdoc />
    public override List<ValidationResult>? GetValidationResults(IEnumerable<TElement>? instance,
        ValidationContext<IEnumerable<TElement>> validationContext)
    {
        // 检查是否是嵌套验证器
        if (instance is null && IsNested)
        {
            return null;
        }

        return GetValidationResults(instance, validationContext.RuleSets);
    }

    /// <inheritdoc />
    public override void Validate(IEnumerable<TElement>? instance,
        ValidationContext<IEnumerable<TElement>> validationContext)
    {
        // 检查是否是嵌套验证器
        if (instance is null && IsNested)
        {
            return;
        }

        Validate(instance, validationContext.RuleSets);
    }

    /// <summary>
    ///     筛选用于执行验证的集合元素
    /// </summary>
    /// <param name="elementFilter">元素过滤委托</param>
    /// <returns>
    ///     <see cref="CollectionValidator{TElement}" />
    /// </returns>
    public CollectionValidator<TElement> Where(Func<TElement, bool>? elementFilter)
    {
        _elementFilter = elementFilter;

        return this;
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

        _elementValidator.Dispose();
    }

    /// <summary>
    ///     筛选用于执行验证的集合元素
    /// </summary>
    /// <param name="elements">
    ///     <see cref="IEnumerable{T}" />
    /// </param>
    /// <returns>
    ///     <see cref="IEnumerable{T}" />
    /// </returns>
    internal IEnumerable<TElement> GetValidatingElements(IEnumerable<TElement> elements)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(elements);

        return _elementFilter is null ? elements : elements.Where(element => _elementFilter(element));
    }

    /// <inheritdoc cref="IMemberPathRepairable.RepairMemberPaths" />
    internal virtual void RepairMemberPaths(string? memberPath) => _memberPath = memberPath;
}