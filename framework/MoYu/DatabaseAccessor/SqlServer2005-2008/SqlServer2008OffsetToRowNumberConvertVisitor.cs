// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
/// 处理 .Skip().Take() 表达式问题
/// </summary>
internal class SqlServer2008OffsetToRowNumberConvertVisitor : ExpressionVisitor
{
#if !NET5_0
    /// <summary>
    /// 筛选列访问器
    /// </summary>
    private static readonly MethodInfo GenerateOuterColumnAccessor;

    /// <summary>
    /// 引用 TableReferenceExpression 类型
    /// </summary>
    private static readonly Type TableReferenceExpressionType;
#else

    /// <summary>
    /// 筛选列访问器
    /// </summary>
    private static readonly Func<SelectExpression, SqlExpression, string, ColumnExpression> GenerateOuterColumnAccessor;

#endif

    /// <summary>
    /// 表达式根节点
    /// </summary>
    private readonly Expression root;

    /// <summary>
    /// Sql 表达式工厂
    /// </summary>
    private readonly ISqlExpressionFactory sqlExpressionFactory;

    /// <summary>
    /// 静态构造函数
    /// </summary>
    static SqlServer2008OffsetToRowNumberConvertVisitor()
    {
        var method = typeof(SelectExpression).GetMethod("GenerateOuterColumn", BindingFlags.NonPublic | BindingFlags.Instance);

        if (!typeof(ColumnExpression).IsAssignableFrom(method?.ReturnType))
            throw new InvalidOperationException("SelectExpression.GenerateOuterColumn() is not found.");

#if !NET5_0
        TableReferenceExpressionType = method.GetParameters().First().ParameterType;
        GenerateOuterColumnAccessor = method;
#else
        GenerateOuterColumnAccessor = (Func<SelectExpression, SqlExpression, string, ColumnExpression>)method.CreateDelegate(typeof(Func<SelectExpression, SqlExpression, string, ColumnExpression>));
#endif
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="root"></param>
    /// <param name="sqlExpressionFactory"></param>
    public SqlServer2008OffsetToRowNumberConvertVisitor(Expression root, ISqlExpressionFactory sqlExpressionFactory)
    {
        this.root = root;
        this.sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <summary>
    /// 替换表达式
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected override Expression VisitExtension(Expression node)
    {
        if (node is ShapedQueryExpression shapedQueryExpression)
        {
            return shapedQueryExpression.Update(Visit(shapedQueryExpression.QueryExpression), shapedQueryExpression.ShaperExpression);
        }
        if (node is SelectExpression se)
            node = VisitSelect(se);
        return base.VisitExtension(node);
    }

    /// <summary>
    /// 更新 Select 语句
    /// </summary>
    /// <param name="selectExpression"></param>
    /// <returns></returns>
    private Expression VisitSelect(SelectExpression selectExpression)
    {
        var oldOffset = selectExpression.Offset;
        if (oldOffset == null)
            return selectExpression;

        var oldLimit = selectExpression.Limit;
        var oldOrderings = selectExpression.Orderings;

        // 在子查询中 OrderBy 必须写 Top 数量
        var newOrderings = oldOrderings.Count > 0 && (oldLimit != null || selectExpression == root)
            ? oldOrderings.ToList()
            : new List<OrderingExpression>();

        // 更新表达式
        selectExpression = selectExpression.Update(selectExpression.Projection.ToList(),
                                                   selectExpression.Tables.ToList(),
                                                   selectExpression.Predicate,
                                                   selectExpression.GroupBy.ToList(),
                                                   selectExpression.Having,
                                                   orderings: newOrderings,
                                                   limit: null,
                                                   offset: null);
        var rowOrderings = oldOrderings.Count != 0 ? oldOrderings
            : new[] { new OrderingExpression(new SqlFragmentExpression("(SELECT 1)"), true) };

        selectExpression.PushdownIntoSubquery();

        var subQuery = (SelectExpression)selectExpression.Tables[0];
        var projection = new RowNumberExpression(Array.Empty<SqlExpression>(), rowOrderings, oldOffset.TypeMapping);
#if !NET5_0
        var left = GenerateOuterColumnAccessor.Invoke(subQuery
            , new object[]
            {
                Activator.CreateInstance(TableReferenceExpressionType, new object[] { subQuery,subQuery.Alias }),
                projection,
                "row",
                true
            }) as ColumnExpression;
#else
        var left = GenerateOuterColumnAccessor(subQuery, projection, "row");
#endif

        selectExpression.ApplyPredicate(sqlExpressionFactory.GreaterThan(left, oldOffset));

        if (oldLimit != null)
        {
            if (oldOrderings.Count == 0)
            {
                selectExpression.ApplyPredicate(sqlExpressionFactory.LessThanOrEqual(left, sqlExpressionFactory.Add(oldOffset, oldLimit)));
            }
            else
            {
                // 这里不支持子查询的 OrderBy 操作
                selectExpression.ApplyLimit(oldLimit);
            }
        }
        return selectExpression;
    }
}