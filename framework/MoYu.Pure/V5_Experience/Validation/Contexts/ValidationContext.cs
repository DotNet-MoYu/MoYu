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

namespace MoYu.Validation;

/// <inheritdoc cref="IValidationContext" />
/// <typeparam name="T">对象类型</typeparam>
public sealed class ValidationContext<T> : IValidationContext, IValidatorInitializer
{
    /// <summary>
    ///     <see cref="IServiceProvider" /> 委托
    /// </summary>
    internal Func<Type, object?>? _serviceProvider;

    /// <summary>
    ///     <inheritdoc cref="ValidationContext{T}" />
    /// </summary>
    /// <param name="instance">对象</param>
    public ValidationContext(T instance)
        // ReSharper disable once IntroduceOptionalParameters.Global
        : this(instance, (Func<Type, object?>?)null, null)
    {
    }

    /// <summary>
    ///     <inheritdoc cref="ValidationContext{T}" />
    /// </summary>
    /// <param name="instance">对象</param>
    /// <param name="items">共享数据</param>
    public ValidationContext(T instance, IDictionary<object, object?>? items)
        : this(instance, (Func<Type, object?>?)null, items)
    {
    }

    /// <summary>
    ///     <inheritdoc cref="ValidationContext{T}" />
    /// </summary>
    /// <param name="instance">对象</param>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    /// <param name="items">共享数据</param>
    public ValidationContext(T instance, IServiceProvider? serviceProvider, IDictionary<object, object?>? items)
        : this(instance, serviceProvider is null ? null : serviceProvider.GetService, items)
    {
    }

    /// <summary>
    ///     <inheritdoc cref="ValidationContext{T}" />
    /// </summary>
    /// <param name="instance">对象</param>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    /// <param name="items">共享数据</param>
    internal ValidationContext(T instance, Func<Type, object?>? serviceProvider, IDictionary<object, object?>? items)
    {
        Instance = instance;
        _serviceProvider = serviceProvider;
        Items = items is not null ? new Dictionary<object, object?>(items) : new Dictionary<object, object?>();
    }

    /// <inheritdoc cref="IValidationContext.Instance" />
    public T Instance { get; }

    /// <inheritdoc />
    object? IValidationContext.Instance => Instance;

    /// <inheritdoc />
    public string DisplayName { get => field ?? MemberNames?.FirstOrDefault() ?? typeof(T).Name; init; }

    /// <inheritdoc />
    public IEnumerable<string>? MemberNames { get; init; }

    /// <inheritdoc />
    public string?[]? RuleSets { get; init; }

    /// <inheritdoc />
    public IDictionary<object, object?> Items { get; }

    /// <inheritdoc />
    public object? GetService(Type serviceType) => _serviceProvider?.Invoke(serviceType);

    /// <inheritdoc />
    void IValidatorInitializer.InitializeServiceProvider(Func<Type, object?>? serviceProvider) =>
        InitializeServiceProvider(serviceProvider);

    /// <inheritdoc cref="IValidatorInitializer.InitializeServiceProvider" />
    internal void InitializeServiceProvider(Func<Type, object?>? serviceProvider) => _serviceProvider = serviceProvider;
}