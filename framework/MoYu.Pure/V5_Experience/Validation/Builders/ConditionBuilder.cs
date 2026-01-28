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

using System.Diagnostics.CodeAnalysis;

namespace MoYu.Validation;

/// <summary>
///     条件规则
/// </summary>
/// <param name="Condition">条件委托</param>
/// <param name="Validators">验证器列表</param>
/// <typeparam name="T">对象类型</typeparam>
internal record ConditionRule<T>(Func<T, bool> Condition, IReadOnlyList<ValidatorBase> Validators);

/// <summary>
///     <see cref="ConditionBuilder{T}" /> 构建结果
/// </summary>
/// <param name="ConditionalRules">条件规则集合</param>
/// <param name="DefaultRules">默认验证规则</param>
/// <typeparam name="T">对象类型</typeparam>
internal record ConditionResult<T>(
    IReadOnlyList<ConditionRule<T>> ConditionalRules,
    IReadOnlyList<ValidatorBase>? DefaultRules);

/// <summary>
///     条件验证构建器
/// </summary>
/// <typeparam name="T">对象类型</typeparam>
public class ConditionBuilder<T>
{
    /// <summary>
    ///     条件规则集合
    /// </summary>
    internal readonly List<ConditionRule<T>> _conditionalRules = [];

    /// <summary>
    ///     默认验证规则
    /// </summary>
    /// <remarks>当所有条件均不满足时使用。</remarks>
    internal List<ValidatorBase>? _defaultRules;

    /// <summary>
    ///     定义满足指定的条件委托
    /// </summary>
    /// <param name="condition">条件委托</param>
    /// <returns>
    ///     <see cref="ConditionThenBuilder{T}" />
    /// </returns>
    public ConditionThenBuilder<T> When(Func<T, bool> condition)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(condition);

        return new ConditionThenBuilder<T>(this, condition);
    }

    /// <summary>
    ///     配置默认验证规则
    /// </summary>
    /// <remarks>当所有条件均不满足时使用。</remarks>
    /// <param name="configure">验证器配置委托</param>
    /// <param name="mode"><see cref="CompositeMode" />，默认值为：<see cref="CompositeMode.FailFast" /></param>
    /// <returns>
    ///     <see cref="ConditionBuilder{T}" />
    /// </returns>
    public ConditionBuilder<T> Otherwise(Action<FluentValidatorBuilder<T>> configure,
        CompositeMode mode = CompositeMode.FailFast)
    {
        _defaultRules = [new CompositeValidator<T>(configure, mode)];

        return this;
    }

    /// <summary>
    ///     配置默认验证规则
    /// </summary>
    /// <remarks>当所有条件均不满足时使用。</remarks>
    /// <param name="validators">验证器列表</param>
    /// <param name="mode"><see cref="CompositeMode" />，默认值为：<see cref="CompositeMode.FailFast" /></param>
    /// <returns>
    ///     <see cref="ConditionBuilder{T}" />
    /// </returns>
    public ConditionBuilder<T> Otherwise(ValidatorBase[] validators, CompositeMode mode = CompositeMode.FailFast)
    {
        _defaultRules = [new CompositeValidator<T>(validators, mode)];

        return this;
    }

    /// <summary>
    ///     配置默认错误信息
    /// </summary>
    /// <remarks>当所有条件均不满足时直接返回该错误信息，不执行任何验证逻辑。</remarks>
    /// <param name="errorMessage">错误信息</param>
    /// <returns>
    ///     <see cref="ConditionBuilder{T}" />
    /// </returns>
    public ConditionBuilder<T> OtherwiseMessage(string? errorMessage)
    {
        _defaultRules = [new FailureValidator().WithMessage(errorMessage)];

        return this;
    }

    /// <summary>
    ///     配置默认错误信息
    /// </summary>
    /// <remarks>当所有条件均不满足时直接返回该错误信息，不执行任何验证逻辑。</remarks>
    /// <param name="resourceType">错误信息资源类型</param>
    /// <param name="resourceName">错误信息资源名称</param>
    /// <returns>
    ///     <see cref="ConditionBuilder{T}" />
    /// </returns>
    public ConditionBuilder<T> OtherwiseMessage([DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.NonPublicProperties)]
        Type resourceType, string resourceName)
    {
        _defaultRules = [new FailureValidator().WithMessage(resourceType, resourceName)];

        return this;
    }

    /// <summary>
    ///     清除
    /// </summary>
    public void Clear()
    {
        _conditionalRules.Clear();

        _defaultRules?.Clear();
        _defaultRules = null;
    }

    /// <summary>
    ///     构建
    /// </summary>
    /// <param name="buildConditions">条件验证构建器配置委托</param>
    /// <returns>
    ///     <see cref="ConditionResult{T}" />
    /// </returns>
    internal ConditionResult<T> Build(Action<ConditionBuilder<T>>? buildConditions = null)
    {
        // 调用条件验证构建器配置委托
        buildConditions?.Invoke(this);

        return new ConditionResult<T>(_conditionalRules, _defaultRules);
    }
}

/// <summary>
///     Then 条件中间构建器
/// </summary>
/// <typeparam name="T">对象类型</typeparam>
public sealed class ConditionThenBuilder<T>
{
    /// <summary>
    ///     条件委托
    /// </summary>
    internal readonly Func<T, bool> _condition;

    /// <inheritdoc cref="ConditionBuilder{T}" />
    internal readonly ConditionBuilder<T> _conditionBuilder;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="conditionBuilder">
    ///     <see cref="ConditionBuilder{T}" />
    /// </param>
    /// <param name="condition">条件委托</param>
    internal ConditionThenBuilder(ConditionBuilder<T> conditionBuilder, Func<T, bool> condition)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(conditionBuilder);
        ArgumentNullException.ThrowIfNull(condition);

        _conditionBuilder = conditionBuilder;
        _condition = condition;
    }

    /// <summary>
    ///     配置满足条件时执行的验证规则
    /// </summary>
    /// <param name="configure">验证器配置委托</param>
    /// <param name="mode"><see cref="CompositeMode" />，默认值为：<see cref="CompositeMode.FailFast" /></param>
    /// <returns>
    ///     <see cref="ConditionBuilder{T}" />
    /// </returns>
    public ConditionBuilder<T> Then(Action<FluentValidatorBuilder<T>> configure,
        CompositeMode mode = CompositeMode.FailFast)
    {
        _conditionBuilder._conditionalRules.Add(new ConditionRule<T>(_condition,
            [new CompositeValidator<T>(configure, mode)]));

        return _conditionBuilder;
    }

    /// <summary>
    ///     配置满足条件时执行的验证规则
    /// </summary>
    /// <param name="validators">验证器列表</param>
    /// <param name="mode"><see cref="CompositeMode" />，默认值为：<see cref="CompositeMode.FailFast" /></param>
    /// <returns>
    ///     <see cref="ConditionBuilder{T}" />
    /// </returns>
    public ConditionBuilder<T> Then(ValidatorBase[] validators, CompositeMode mode = CompositeMode.FailFast)
    {
        _conditionBuilder._conditionalRules.Add(new ConditionRule<T>(_condition,
            [new CompositeValidator<T>(validators, mode)]));

        return _conditionBuilder;
    }

    /// <summary>
    ///     配置满足条件时直接返回指定的错误信息
    /// </summary>
    /// <remarks>不执行任何验证逻辑，仅用于输出错误提示。</remarks>
    /// <param name="errorMessage">错误信息</param>
    /// <returns>
    ///     <see cref="ConditionBuilder{T}" />
    /// </returns>
    public ConditionBuilder<T> ThenMessage(string? errorMessage)
    {
        _conditionBuilder._conditionalRules.Add(new ConditionRule<T>(_condition,
            [new FailureValidator().WithMessage(errorMessage)]));

        return _conditionBuilder;
    }

    /// <summary>
    ///     配置满足条件时直接返回指定的错误信息
    /// </summary>
    /// <remarks>不执行任何验证逻辑，仅用于输出错误提示。</remarks>
    /// <param name="resourceType">错误信息资源类型</param>
    /// <param name="resourceName">错误信息资源名称</param>
    /// <returns>
    ///     <see cref="ConditionBuilder{T}" />
    /// </returns>
    public ConditionBuilder<T> ThenMessage(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties |
                                    DynamicallyAccessedMemberTypes.NonPublicProperties)]
        Type resourceType, string resourceName)
    {
        _conditionBuilder._conditionalRules.Add(new ConditionRule<T>(_condition,
            [new FailureValidator().WithMessage(resourceType, resourceName)]));

        return _conditionBuilder;
    }
}