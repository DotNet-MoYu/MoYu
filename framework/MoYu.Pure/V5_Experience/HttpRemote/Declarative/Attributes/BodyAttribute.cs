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

namespace MoYu.HttpRemote;

/// <summary>
///     HTTP 声明式请求内容特性
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class BodyAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="BodyAttribute" />
    /// </summary>
    public BodyAttribute()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="BodyAttribute" />
    /// </summary>
    /// <param name="contentType">内容类型</param>
    public BodyAttribute(string contentType) => ContentType = contentType;

    /// <summary>
    ///     <inheritdoc cref="QueryAttribute" />
    /// </summary>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    public BodyAttribute(string contentType, string contentEncoding)
        : this(contentType) =>
        ContentEncoding = contentEncoding;

    /// <summary>
    ///     内容类型
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    ///     内容编码
    /// </summary>
    public string? ContentEncoding { get; set; }

    /// <summary>
    ///     是否使用 <see cref="StringContent" /> 构建 <see cref="FormUrlEncodedContent" />。默认 <c>false</c>
    /// </summary>
    /// <remarks>当 <see cref="ContentType" /> 值为 <c>application/x-www-form-urlencoded</c> 时有效。</remarks>
    public bool UseStringContent { get; set; }

    /// <summary>
    ///     是否为原始字符串内容。默认 <c>false</c>
    /// </summary>
    /// <remarks>
    ///     <para>作用于 <see cref="string" /> 类型参数时有效。</para>
    ///     <para>当属性值设置为 <c>true</c> 时，将校验 <see cref="ContentType" /> 属性值是否为空，并且字符串内容将被双引号包围并发送，格式如下：<c>"内容"</c>。</para>
    /// </remarks>
    public bool RawString { get; set; }
}