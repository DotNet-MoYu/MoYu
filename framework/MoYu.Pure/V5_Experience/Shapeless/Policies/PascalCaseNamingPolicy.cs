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

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MoYu.Shapeless;

/// <summary>
///     帕斯卡（大驼峰）命名策略
/// </summary>
public sealed partial class PascalCaseNamingPolicy : JsonNamingPolicy
{
    /// <inheritdoc />
    public override string ConvertName(string name)
    {
        // 空检查
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        // 对于全部大写的单词（如 'URL', 'ID'），保持不变
        if (name.All(char.IsUpper) && !name.Any(char.IsDigit))
        {
            return name;
        }

        // 初始化 StringBuilder 实例
        var result = new StringBuilder();

        // 将字符串按非字母数字字符、大小写字母变化处分割成多个部分
        var parts = WordBoundaryRegex().Split(name);

        // 遍历并逐个处理各个部分
        foreach (var part in parts)
        {
            // 空检查
            if (string.IsNullOrWhiteSpace(part))
            {
                continue;
            }

            // 如果是连续的大写字母，假设是缩写，保持不变
            if (part.Length > 1 && part.All(char.IsUpper))
            {
                result.Append(part);
            }
            else
            {
                // 对每个部分的第一个字母进行大写转换，其余小写
                result.Append(char.ToUpper(part[0]));

                if (part.Length > 1)
                {
                    result.Append(part[1..].ToLower());
                }
            }
        }

        return result.ToString();
    }

    /// <summary>
    ///     单词边界正则表达式
    /// </summary>
    /// <returns>
    ///     <see cref="Regex" />
    /// </returns>
    [GeneratedRegex(
        @"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])|(?<=\d)(?=\D)|(?<=\D)(?=\d)")]
    private static partial Regex WordBoundaryRegex();
}