// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using Microsoft.AspNetCore.Localization;

namespace MoYu.Localization;

/// <summary>
/// 自定义多语言查询参数
/// </summary>
public class CustomizeQueryStringRequestCultureProvider : QueryStringRequestCultureProvider
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="queryStringKey"></param>
    /// <param name="uiQueryStringKey"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public CustomizeQueryStringRequestCultureProvider(string queryStringKey, string uiQueryStringKey = null)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(queryStringKey)) throw new ArgumentNullException(nameof(queryStringKey));

        QueryStringKey = queryStringKey;
        UIQueryStringKey = string.IsNullOrWhiteSpace(uiQueryStringKey) ? $"ui-{queryStringKey}" : uiQueryStringKey;
    }
}