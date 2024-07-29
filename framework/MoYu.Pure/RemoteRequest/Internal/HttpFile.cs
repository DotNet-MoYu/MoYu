// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

namespace MoYu.RemoteRequest;

/// <summary>
/// 远程请求文件类
/// </summary>
[SuppressSniffer]
public sealed class HttpFile
{
    /// <summary>
    /// 创建 HttpFile 类
    /// </summary>
    /// <param name="name"></param>
    /// <param name="bytes"></param>
    /// <param name="fileName"></param>
    /// <param name="escape"></param>
    /// <returns></returns>
    public static HttpFile Create(string name, byte[] bytes, string fileName = default, bool escape = true)
    {
        return new HttpFile
        {
            Name = name,
            Bytes = bytes,
            FileName = fileName,
            Escape = escape
        };
    }

    /// <summary>
    /// 创建 HttpFile 类
    /// </summary>
    /// <param name="name"></param>
    /// <param name="fileStream"></param>
    /// <param name="fileName"></param>
    /// <param name="escape"></param>
    /// <returns></returns>
    public static HttpFile Create(string name, Stream fileStream, string fileName = default, bool escape = true)
    {
        return new HttpFile
        {
            Name = name,
            FileStream = fileStream,
            FileName = fileName,
            Escape = escape
        };
    }

    /// <summary>
    /// 添加多个文件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public static HttpFile[] CreateMultiple(string name, params (byte[] bytes, string fileName)[] items)
    {
        var files = new List<HttpFile>();
        if (items == null || items.Length == 0) return files.ToArray();

        foreach (var (bytes, fileName) in items)
        {
            files.Add(Create(name, bytes, fileName));
        }

        return files.ToArray();
    }

    /// <summary>
    /// 添加多个文件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public static HttpFile[] CreateMultiple(string name, params (Stream fileStream, string fileName)[] items)
    {
        var files = new List<HttpFile>();
        if (items == null || items.Length == 0) return files.ToArray();

        foreach (var (fileStream, fileName) in items)
        {
            files.Add(Create(name, fileStream, fileName));
        }

        return files.ToArray();
    }

    /// <summary>
    /// 表单名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 文件字节数组
    /// </summary>
    public byte[] Bytes { get; set; }

    /// <summary>
    /// 文件流
    /// </summary>
    public Stream FileStream { get; set; }

    /// <summary>
    /// 是否对表单名/文件名进行转义
    /// </summary>
    /// <remarks>默认 true</remarks>
    public bool Escape { get; set; } = true;
}