// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using System.Collections.Generic;

namespace MoYu.Tools.CommandLine;

/// <summary>
/// 参数模型
/// </summary>
public sealed class ArgumentModel
{
    /// <summary>
    /// 参数字典
    /// </summary>
    public Dictionary<string, object> ArgumentDictionary { get; internal set; }

    /// <summary>
    /// 参数键值对
    /// </summary>
    public List<KeyValuePair<string, string>> ArgumentList { get; internal set; }

    /// <summary>
    /// 参数命令
    /// </summary>
    public string CommandLineString { get; internal set; }

    /// <summary>
    /// 操作符列表
    /// </summary>
    public List<string> OperandList { get; internal set; }
}
