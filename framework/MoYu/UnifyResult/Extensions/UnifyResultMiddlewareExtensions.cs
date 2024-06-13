// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using MoYu.UnifyResult;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 状态码中间件拓展
/// </summary>
[SuppressSniffer]
public static class UnifyResultMiddlewareExtensions
{
    /// <summary>
    /// 添加状态码拦截中间件
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="authorizedHeaders"></param>
    /// <param name="withAuthorizationHeaderCheck"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseUnifyResultStatusCodes(this IApplicationBuilder builder, string[] authorizedHeaders = null, bool withAuthorizationHeaderCheck = false)
    {
        // 注册中间件
        UnifyContext.EnabledStatusCodesMiddleware = true;   // 设置标识

        // 设置授权验证失败识别头，如果不匹配将不进入规范化处理，主要解决 Windows 域授权或其他授权重新发起失败问题
        var checkAuthorizedHeaders = (authorizedHeaders ?? Array.Empty<string>()).Concat(new[] { "WWW-Authenticate" }).ToArray();

        builder.UseMiddleware<UnifyResultStatusCodesMiddleware>(new object[] { checkAuthorizedHeaders, withAuthorizationHeaderCheck });

        return builder;
    }
}