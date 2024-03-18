// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

namespace MoYu.SensitiveDetection;

/// <summary>
/// 脱敏词汇构建器
/// </summary>
[SuppressSniffer]
public sealed class SensitiveDetectionBuilder
{
    /// <summary>
    /// 脱敏词汇数据文件名
    /// </summary>
    public string EmbedFileName { get; set; } = "sensitive-words.txt";
}