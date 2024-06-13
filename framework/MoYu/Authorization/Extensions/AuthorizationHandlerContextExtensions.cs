// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Authorization;

/// <summary>
/// 授权处理上下文拓展类
/// </summary>
[SuppressSniffer]
public static class AuthorizationHandlerContextExtensions
{
    internal const string FAIL_STATUSCODE_KEY = $"{nameof(AuthorizationHandlerContext)}_FAIL_STATUSCODE";

    /// <summary>
    /// 获取当前 HttpContext 上下文
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static DefaultHttpContext GetCurrentHttpContext(this AuthorizationHandlerContext context)
    {
        DefaultHttpContext httpContext;

        // 获取 httpContext 对象
        if (context.Resource is AuthorizationFilterContext filterContext) httpContext = (DefaultHttpContext)filterContext.HttpContext;
        else if (context.Resource is DefaultHttpContext defaultHttpContext) httpContext = defaultHttpContext;
        else httpContext = null;

        return httpContext;
    }

    /// <summary>
    /// 设置授权状态码
    /// </summary>
    /// <param name="context"></param>
    /// <param name="statusCode"></param>
    public static void StatusCode(this AuthorizationHandlerContext context, int statusCode)
    {
        var httpContext = context.GetCurrentHttpContext();
        if (httpContext != null)
        {
            httpContext.Items[FAIL_STATUSCODE_KEY] = statusCode;
        }
    }

    /// <summary>
    /// 标记授权失败并设置状态码
    /// </summary>
    /// <param name="context"></param>
    /// <param name="statusCode"></param>
    public static void Fail(this AuthorizationHandlerContext context, int statusCode)
    {
        context.Fail();
        context.StatusCode(statusCode);
    }
}