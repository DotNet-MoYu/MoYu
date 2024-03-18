// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
/// SqlServer 查询转换工厂（处理 SqlServer 2008 分页问题）
/// </summary>
[SuppressSniffer]
public class SqlServer2008QueryTranslationPostprocessorFactory : IQueryTranslationPostprocessorFactory
{
    /// <summary>
    /// 查询转换依赖集合
    /// </summary>
    private readonly QueryTranslationPostprocessorDependencies _dependencies;

    /// <summary>
    /// 关系查询转换依赖集合
    /// </summary>
    private readonly RelationalQueryTranslationPostprocessorDependencies _relationalDependencies;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="relationalDependencies"></param>
    public SqlServer2008QueryTranslationPostprocessorFactory(QueryTranslationPostprocessorDependencies dependencies, RelationalQueryTranslationPostprocessorDependencies relationalDependencies)
    {
        _dependencies = dependencies;
        _relationalDependencies = relationalDependencies;
    }

    /// <summary>
    /// 创建查询转换实例工厂
    /// </summary>
    /// <param name="queryCompilationContext"></param>
    /// <returns></returns>
    public virtual QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
    {
        return new SqlServer2008QueryTranslationPostprocessor(
              _dependencies,
              _relationalDependencies,
              queryCompilationContext);
    }
}