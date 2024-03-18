// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using System.Collections.Concurrent;
using System.Reflection;

namespace Dapper;

/// <summary>
/// Sql 类型
/// </summary>
public static class SqlProvider
{
    /// <summary>
    /// SqlServer 提供器程序集
    /// </summary>
    public const string SqlServer = "Microsoft.Data.SqlClient";

    /// <summary>
    /// Sqlite 提供器程序集
    /// </summary>
    public const string Sqlite = "Microsoft.Data.Sqlite";

    /// <summary>
    /// MySql 提供器程序集
    /// </summary>
    public const string MySql = "MySql.Data";

    /// <summary>
    /// PostgreSQL 提供器程序集
    /// </summary>
    public const string Npgsql = "Npgsql";

    /// <summary>
    /// Oracle 提供器程序集
    /// </summary>
    public const string Oracle = "Oracle.ManagedDataAccess";

    /// <summary>
    /// Firebird 提供器程序集
    /// </summary>
    public const string Firebird = "FirebirdSql.Data.FirebirdClient";

    /// <summary>
    /// 数据库提供器连接对象类型集合
    /// </summary>
    internal static readonly ConcurrentDictionary<string, Type> SqlProviderDbConnectionTypeCollection;

    /// <summary>
    /// 静态构造函数
    /// </summary>
    static SqlProvider()
    {
        SqlProviderDbConnectionTypeCollection = new ConcurrentDictionary<string, Type>();
    }

    /// <summary>
    /// 获取数据库连接对象类型
    /// </summary>
    /// <param name="sqlProvider"></param>
    /// <returns></returns>
    internal static Type GetDbConnectionType(string sqlProvider)
    {
        return SqlProviderDbConnectionTypeCollection.GetOrAdd(sqlProvider, Function);

        // 本地静态方法
        static Type Function(string sqlProvider)
        {
            // 加载对应的数据库提供器程序集
            var databaseProviderAssembly = Assembly.Load(sqlProvider);

            // 获取对应数据库连接对象
            var databaseDbConnectionTypeName = sqlProvider switch
            {
                SqlServer => "Microsoft.Data.SqlClient.SqlConnection",
                Sqlite => "Microsoft.Data.Sqlite.SqliteConnection",
                MySql => "MySql.Data.MySqlClient.MySqlConnection",
                Npgsql => "Npgsql.NpgsqlConnection",
                Oracle => "Oracle.ManagedDataAccess.Client.OracleConnection",
                Firebird => "FirebirdSql.Data.FirebirdClient.FbConnection",
                _ => null
            };

            // 加载数据库连接对象类型
            var dbConnectionType = databaseProviderAssembly.GetType(databaseDbConnectionTypeName);

            return dbConnectionType;
        }
    }
}