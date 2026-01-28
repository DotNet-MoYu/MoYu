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
using System.Reflection;

namespace MoYu.Extensions;

/// <summary>
///     <see cref="Expression" /> 扩展类
/// </summary>
internal static class LinqExpressionExtensions
{
    /// <summary>
    ///     根据条件成立构建 <c>Where</c> 表达式
    /// </summary>
    /// <param name="source">
    ///     <see cref="IEnumerable{TSource}" />
    /// </param>
    /// <param name="condition">条件</param>
    /// <param name="predicate"><c>Where</c> 表达式</param>
    /// <typeparam name="TSource">集合元素类型</typeparam>
    /// <returns>
    ///     <see cref="IEnumerable{TSource}" />
    /// </returns>
    internal static IEnumerable<TSource> WhereIf<TSource>(this IEnumerable<TSource> source
        , bool condition
        , Func<TSource, bool> predicate)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(predicate);

        return !condition
            ? source
            : source.Where(predicate);
    }

    /// <summary>
    ///     解析表达式并获取属性的 <see cref="PropertyInfo" /> 实例
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <typeparam name="TProperty">属性类型</typeparam>
    /// <param name="propertySelector">
    ///     <see cref="Expression{TDelegate}" />
    /// </param>
    /// <returns>
    ///     <see cref="PropertyInfo" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    internal static PropertyInfo GetProperty<T, TProperty>(this Expression<Func<T, TProperty>> propertySelector) =>
        propertySelector.Body switch
        {
            // 检查 Lambda 表达式的主体是否是 MemberExpression 类型
            MemberExpression memberExpression => GetProperty<T>(memberExpression),
            // 如果主体是 UnaryExpression 类型，则继续解析
            UnaryExpression { Operand: MemberExpression nestedMemberExpression } => GetProperty<T>(
                nestedMemberExpression),
            _ => throw new ArgumentException("Expression must be a simple member access (e.g. x => x.Property).",
                nameof(propertySelector))
        };

    /// <summary>
    ///     从成员表达式中提取 <see cref="PropertyInfo" /> 实例
    /// </summary>
    /// <param name="memberExpression">
    ///     <see cref="MemberExpression" />
    /// </param>
    /// <typeparam name="T">对象类型</typeparam>
    /// <returns>
    ///     <see cref="PropertyInfo" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    internal static PropertyInfo GetProperty<T>(MemberExpression memberExpression)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(memberExpression);

        // 确保表达式根是 T 类型的参数
        if (memberExpression.Expression is not ParameterExpression parameterExpression ||
            parameterExpression.Type != typeof(T))
        {
            throw new ArgumentException(
                $"Expression '{memberExpression}' must refer to a member of type '{typeof(T)}'.",
                nameof(memberExpression));
        }

        // 确保成员是属性（非字段）
        if (memberExpression.Member is not PropertyInfo propertyInfo)
        {
            throw new ArgumentException(
                $"Expression '{memberExpression}' refers to a field. Only properties are supported.",
                nameof(memberExpression));
        }

        return propertyInfo;
    }
}