// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

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
            var embedFileNameOfResource = $"{Reflect.GetAssemblyName(entryAssembly)}.{_embedFileName}";

            // 解析嵌入式文件流
            byte[] buffer;
            using (var readStream = entryAssembly.GetManifestResourceStream(embedFileNameOfResource))
            {
                if (readStream == null)
                {
                    throw new InvalidOperationException($"The embedded file of path <{embedFileNameOfResource}> is not found.");
                }

                buffer = new byte[readStream.Length];
                await readStream.ReadAsync(buffer.AsMemory(0, buffer.Length));
            }

            // 同时兼容 UTF-8 BOM，UTF-8
            using (var stream = new MemoryStream(buffer))
            {
                using var streamReader = new StreamReader(stream, new UTF8Encoding(false));
                wordsOfCached = await streamReader.ReadToEndAsync();
            }

            // 缓存数据
            await _distributedCache.SetStringAsync(DISTRIBUTED_KEY, wordsOfCached);
        }

        // 取换行符分割字符串
        var words = wordsOfCached.Split(new[] { "\r\n", "|" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(u => u.Trim())
            .Distinct();

        return words;
    }

    /// <summary>
    /// 判断脱敏词汇是否有效（支持自定义算法）
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public async Task<bool> VaildedAsync(string text)
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
        if (string.IsNullOrWhiteSpace(text)) return default;

        // 查找脱敏词汇出现次数和位置
        var foundSets = await FoundSensitiveWordsAsync(text);

        // 如果没有敏感词则返回原字符串
        if (foundSets.Count == 0) return text;

        var stringBuilder = new StringBuilder(text);

        // 循环替换
        foreach (var kv in foundSets)
        {
            for (var i = 0; i < kv.Value.Count; i++)
            {
                for (var j = 0; j < kv.Key.Length; j++)
                {
                    var tempIndex = GetSensitiveWordIndex(kv.Value, i, sensitiveWordLength: kv.Key.Length);

                    // 设置替换的字符
                    stringBuilder[tempIndex + j] = transfer;
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

        var stringBuilder = new StringBuilder(realText);
        var tempStringBuilder = new StringBuilder();

        // 记录脱敏词汇出现位置和次数
        int findIndex;
        var foundSets = new Dictionary<string, List<int>>();

        // 遍历所有脱敏词汇并查找字符串是否包含
        foreach (var sensitiveWord in sensitiveWords)
        {
            // 重新填充目标字符串
            tempStringBuilder.Clear();
            tempStringBuilder.Append(stringBuilder);

            // 查询查找至结尾
            while (tempStringBuilder.ToString().IndexOf(sensitiveWord) > -1)
            {
                if (foundSets.ContainsKey(sensitiveWord) == false)
                {
                    foundSets.Add(sensitiveWord, new());
                }

                findIndex = tempStringBuilder.ToString().IndexOf(sensitiveWord);
                foundSets[sensitiveWord].Add(findIndex);

                // 删除从零开始，长度为 findIndex + sensitiveWord.Length 的字符串
                tempStringBuilder.Remove(0, findIndex + sensitiveWord.Length);
            }
        }

        return foundSets;
    }

    /// <summary>
    /// 获取敏感词索引
    /// </summary>
    /// <param name="list"></param>
    /// <param name="count"></param>
    /// <param name="sensitiveWordLength"></param>
    /// <returns></returns>
    private static int GetSensitiveWordIndex(List<int> list, int count, int sensitiveWordLength)
    {
        // 用于返回当前敏感词的第 count 个的真实索引
        var sum = 0;
        for (var i = 0; i <= count; i++)
        {
            if (i == 0)
            {
                sum = list[i];
            }
            else
            {
                sum += list[i] + sensitiveWordLength;
            }
        }
        return sum;
    }
}