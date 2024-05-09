// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using System.Text;

namespace MoYu.RemoteRequest;

/// <summary>
/// HTTP 响应模型
/// </summary>
/// <typeparam name="T">返回类型</typeparam>
[SuppressSniffer]
public class HttpResponseModel<T> : IDisposable
{
    /// <summary>
    /// <see cref="HttpResponseMessage"/>
    /// </summary>
    public HttpResponseMessage Response { get; set; }

    /// <summary>
    /// 内容编码
    /// </summary>
    public Encoding Encoding { get; set; }

    /// <summary>
    /// 返回结果
    /// </summary>
    public T Result { get; set; }

    ///<inheritdoc/>
    public void Dispose()
    {
        Response?.Content?.Dispose();
        Response?.Dispose();
    }
}