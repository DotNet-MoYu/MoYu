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

using MoYu.Reflection;
using MoYu.Templates.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace MoYu.SensitiveDetection;

/// <summary>
/// 脱敏词汇（脱敏）提供器（默认实现）
/// </summary>
[SuppressSniffer]
public class SensitiveDetectionProvider : ISensitiveDetectionProvider
{
    /// <summary>
    /// 分布式缓存
    /// </summary>
    private readonly IDistributedCache _distributedCache;

    /// <summary>
    /// 脱敏词汇数据文件名
    /// </summary>
    private readonly string _embedFileName;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="distributedCache"></param>
    /// <param name="embedFileName"></param>
    public SensitiveDetectionProvider(IDistributedCache distributedCache
        , string embedFileName)
    {
        _distributedCache = distributedCache;
        _embedFileName = embedFileName;
    }

    /// <summary>
    /// 分布式缓存键
    /// </summary>
    private const string DISTRIBUTED_KEY = "SENSITIVE:WORDS";

    /// <summary>
    /// 返回所有脱敏词汇
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<string>> GetWordsAsync()
    {
        // 读取缓存数据
        var wordsOfCached = await _distributedCache.GetStringAsync(DISTRIBUTED_KEY);
        if (wordsOfCached == null)
        {
            var entryAssembly = Reflect.GetEntryAssembly();

            /*
             * 查找脱敏词汇数据的嵌入资源文件。
             * 由于程序集名称可在 .csproj 文件中通过 <AssemblyName> 自定义，
             * 故采用模糊匹配方式查找。请确保资源文件名具有唯一性，避免歧义。
             * 
             * var embedFileNameOfResource = $"{Reflect.GetAssemblyName(entryAssembly)}.{_embedFileName}";
             */
            var embedFileNameOfResource = entryAssembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(_embedFileName, StringComparison.OrdinalIgnoreCase));

            // 解析嵌入式文件流
            byte[] buffer;
            using (var readStream = entryAssembly.GetManifestResourceStream(embedFileNameOfResource))
            {
                if (readStream == null)
                {
                    throw new InvalidOperationException($"The embedded file of path <{embedFileNameOfResource}> is not found.");
                }

                buffer = new byte[readStream.Length];
                var position = 0;
                var remaining = buffer.Length;
                while (remaining > 0)
                {
                    var read = await readStream.ReadAsync(buffer.AsMemory(position, remaining));
                    if (read == 0) break; // 流结束
                    position += read;
                    remaining -= read;
                }
            }

            // 同时兼容 UTF-8 BOM，UTF-8
            using (var stream = new MemoryStream(buffer))
            using (var streamReader = new StreamReader(stream, new UTF8Encoding(true)))
            {
                wordsOfCached = await streamReader.ReadToEndAsync();
            }

            // 缓存数据
            await _distributedCache.SetStringAsync(DISTRIBUTED_KEY, wordsOfCached);
        }

        // 统一换行符：先将 \r\n 和 \r 标准化为 \n，再按 \n 和 | 分割
        // 解决跨平台换行符差异导致词汇分割失败的问题
        var normalizedText = wordsOfCached.Replace("\r\n", "\n").Replace("\r", "\n");
        var words = normalizedText.Split(new[] { "\n", "|" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(u => u.Trim())
            .Where(u => !string.IsNullOrEmpty(u))
            .Distinct();

        return words;
    }

    /// <summary>
    /// 判断脱敏词汇是否有效（支持自定义算法）
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public async Task<bool> IsValidAsync(string text)
    {
        // 空字符串和空白字符不验证
        if (string.IsNullOrWhiteSpace(text)) return true;

        // 查找脱敏词汇出现次数和位置
        var foundSets = await FoundSensitiveWordsAsync(text);

        return foundSets.Count == 0;
    }

    /// <summary>
    /// 替换敏感词汇
    /// </summary>
    /// <param name="text"></param>
    /// <param name="transfer"></param>
    /// <returns></returns>
    public async Task<string> ReplaceAsync(string text, char transfer = '*')
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        // 查找脱敏词汇出现次数和位置
        var foundSets = await FoundSensitiveWordsAsync(text);

        // 如果没有敏感词则返回原字符串
        if (foundSets.Count == 0) return text;

        var stringBuilder = new StringBuilder(text);

        // 从后往前替换，避免前面的替换影响后面敏感词的位置
        foreach (var kv in foundSets)
        {
            var sensitiveWord = kv.Key;
            // 按位置降序排列，确保从后往前替换
            var positions = kv.Value.OrderByDescending(p => p);

            foreach (var position in positions)
            {
                for (var i = 0; i < sensitiveWord.Length; i++)
                {
                    stringBuilder[position + i] = transfer;
                }
            }
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 查找脱敏词汇
    /// </summary>
    /// <param name="text"></param>
    public async Task<Dictionary<string, List<int>>> FoundSensitiveWordsAsync(string text)
    {
        // 支持读取配置渲染
        var realText = text.Render();

        // 获取脱敏词库
        var sensitiveWords = await GetWordsAsync();

        // 记录脱敏词汇出现位置和次数
        var foundSets = new Dictionary<string, List<int>>();

        // 遍历所有脱敏词汇并查找字符串是否包含
        foreach (var sensitiveWord in sensitiveWords)
        {
            if (string.IsNullOrEmpty(sensitiveWord)) continue;

            var startIndex = 0;
            while (true)
            {
                // 在原始字符串中直接查找，记录绝对位置
                var findIndex = realText.IndexOf(sensitiveWord, startIndex, StringComparison.OrdinalIgnoreCase);
                if (findIndex == -1) break;

                if (!foundSets.TryGetValue(sensitiveWord, out var value))
                {
                    value = [];
                    foundSets[sensitiveWord] = value;
                }

                value.Add(findIndex);

                // 从下一个字符开始继续查找，支持重叠匹配
                startIndex = findIndex + 1;
            }
        }

        return foundSets;
    }
}