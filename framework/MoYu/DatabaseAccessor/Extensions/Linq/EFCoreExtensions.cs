// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// EntityFramework Core 拓展
/// </summary>
[SuppressSniffer]
public static class EFCoreExtensions
{
    /// <summary>
    /// 根据条件成立再构建 Include 查询
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <typeparam name="TProperty">泛型属性类型</typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="expression">新的集合对象表达式</param>
    /// <returns></returns>
    public static IQueryable<TSource> Include<TSource, TProperty>(this IQueryable<TSource> sources, bool condition, Expression<Func<TSource, TProperty>> expression) where TSource : class
    {
        return condition ? sources.Include(expression) : sources;
    }

    /// <summary>
    /// 构建 OrderBy 查询（自动处理 N 级）
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="keySelector">表达式</param>
    /// <returns>新的集合对象</returns>
    public static IOrderedQueryable<TSource> FlexOrderBy<TSource, TKey>(this IQueryable<TSource> sources, Expression<Func<TSource, TKey>> keySelector)
    {
        return sources.Expression.Type.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>)
               ? ((IOrderedQueryable<TSource>)sources).ThenBy(keySelector)
               : sources.OrderBy(keySelector);
    }

    /// <summary>
    /// 构建 OrderByDescending 查询（自动处理 N 级）
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="keySelector">表达式</param>
    /// <returns>新的集合对象</returns>
    public static IOrderedQueryable<TSource> FlexOrderByDescending<TSource, TKey>(this IQueryable<TSource> sources, Expression<Func<TSource, TKey>> keySelector)
    {
        return sources.Expression.Type.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>)
               ? ((IOrderedQueryable<TSource>)sources).ThenByDescending(keySelector)
               : sources.OrderByDescending(keySelector);
    }

    /// <summary>
    /// 根据条件成立再构建 OrderBy 查询
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="keySelector">表达式</param>
    /// <returns>新的集合对象</returns>
    public static IQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> sources, bool condition, Expression<Func<TSource, TKey>> keySelector)
    {
        return condition ? sources.FlexOrderBy(keySelector) : sources;
    }

    /// <summary>
    /// 根据条件成立再构建 OrderByDescending 查询
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="keySelector">表达式</param>
    /// <returns>新的集合对象</returns>
    public static IQueryable<TSource> OrderByDescending<TSource, TKey>(this IQueryable<TSource> sources, bool condition, Expression<Func<TSource, TKey>> keySelector)
    {
        return condition ? sources.FlexOrderByDescending(keySelector) : sources;
    }

    /// <summary>
    /// 根据条件成立再构建 ThenBy 查询
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="keySelector">表达式</param>
    /// <returns>新的集合对象</returns>
    public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(this IOrderedQueryable<TSource> sources, bool condition, Expression<Func<TSource, TKey>> keySelector)
    {
        return condition ? sources.ThenBy(keySelector) : sources;
    }

    /// <summary>
    /// 根据条件成立再构建 ThenByDescending 查询
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="keySelector">表达式</param>
    /// <returns>新的集合对象</returns>
    public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey>(this IOrderedQueryable<TSource> sources, bool condition, Expression<Func<TSource, TKey>> keySelector)
    {
        return condition ? sources.ThenByDescending(keySelector) : sources;
    }
}