// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using MoYu.FriendlyException;
using MoYu.UnifyResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MoYu.Authorization;

/// <summary>
/// 授权策略执行程序
/// </summary>
[SuppressSniffer]
public abstract class AppAuthorizeHandler : IAuthorizationHandler
{
    /// <summary>
    /// 刷新 Token 身份标识
    /// </summary>
    private readonly string[] _refreshTokenClaims = new[] { "f", "e", "s", "l", "k" };

    /// <summary>
    /// 授权验证核心方法
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        // 获取 HttpContext 上下文
        var httpContext = context.GetCurrentHttpContext();

        try
        {
            await HandleAsync(context, httpContext);
        }
        catch (Exception exception)
        {
            context.Fail();

            // 处理规范化结果
            await UnifyWrapper(httpContext, exception);
        }
    }

    /// <summary>
    /// 授权验证核心方法（可重写）
    /// </summary>
    /// <param name="context"></param>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public virtual async Task HandleAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
    {
        // 判断是否授权
        var isAuthenticated = context.User.Identity.IsAuthenticated;
        if (isAuthenticated)
        {
            // 禁止使用刷新 Token 进行单独校验
            if (_refreshTokenClaims.All(k => context.User.Claims.Any(c => c.Type == k)))
            {
                context.Fail();
                return;
            }

            await AuthorizeHandleAsync(context);
        }
        else context.GetCurrentHttpContext()?.SignoutToSwagger();    // 退出 Swagger 登录
    }

    /// <summary>
    /// 验证管道
    /// </summary>
    /// <param name="context"></param>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public virtual Task<bool> PipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// 策略验证管道
    /// </summary>
    /// <param name="context"></param>
    /// <param name="httpContext"></param>
    /// <param name="requirement"></param>
    /// <returns></returns>
    public virtual Task<bool> PolicyPipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext, IAuthorizationRequirement requirement)
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// 授权处理
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected async Task AuthorizeHandleAsync(AuthorizationHandlerContext context)
    {
        // 获取所有未成功验证的需求
        var pendingRequirements = context.PendingRequirements;

        // 获取 HttpContext 上下文
        var httpContext = context.GetCurrentHttpContext();

        // 调用子类管道
        var pipeline = await PipelineAsync(context, httpContext);
        if (pipeline)
        {
            // 通过授权验证
            foreach (var requirement in pendingRequirements)
            {
                // 验证策略管道
                var policyPipeline = await PolicyPipelineAsync(context, httpContext, requirement);
                if (policyPipeline) context.Succeed(requirement);
                else context.Fail();
            }
        }
        else context.Fail();
    }

    /// <summary>
    /// 处理规范化结果
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    private static async Task UnifyWrapper(DefaultHttpContext httpContext, Exception exception)
    {
        // 尝试解析为友好异常
        var friendlyException = exception as AppFriendlyException;

        // 处理规范化结果
        if (!UnifyContext.CheckExceptionHttpContextNonUnify(httpContext, out var unifyRes))
        {
            _ = UnifyContext.CheckVaildResult(unifyRes.OnAuthorizeException(httpContext, new ExceptionMetadata
            {
                StatusCode = friendlyException?.StatusCode ?? StatusCodes.Status500InternalServerError,
                Errors = friendlyException?.ErrorMessage ?? exception.Message,
                Data = friendlyException?.Data,
                ErrorCode = friendlyException?.ErrorCode,
                OriginErrorCode = friendlyException?.OriginErrorCode,
                Exception = exception
            }), out var data);

            // 终止返回
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(data, App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
        }
        else throw exception;
    }
}