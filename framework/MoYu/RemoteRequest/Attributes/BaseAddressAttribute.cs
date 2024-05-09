// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

namespace MoYu.RemoteRequest;

/// <summary>
/// 配置客户端 BaseAddress
/// </summary>
[SuppressSniffer, AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public class BaseAddressAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="baseAddress"></param>
    public BaseAddressAttribute(string baseAddress)
    {
        BaseAddress = baseAddress;
    }

    /// <summary>
    /// 客户端名称
    /// </summary>
    public string BaseAddress { get; set; }
}