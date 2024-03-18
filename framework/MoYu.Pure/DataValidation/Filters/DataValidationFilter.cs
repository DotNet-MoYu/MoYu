// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using MoYu.DynamicApiController;
using MoYu.FriendlyException;
using MoYu.UnifyResult;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace MoYu.DataValidation;

/// <summary>
/// 数据验证拦截器
/// </summary>
[SuppressSniffer]
public sealed class DataValidationFilter : IAsyncActionFilter, IOrderedFilter
{
    /// <summary>
    /// Api 行为配置选项
    /// </summary>
    private readonly ApiBehaviorOptions _apiBehaviorOptions;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options"></param>
    public DataValidationFilter(IOptions<ApiBehaviorOptions> options)
    {
        _apiBehaviorOptions = options.Value;
    }

    /// <summary>
    /// 过滤器排序
    /// </summary>
    private const int FilterOrder = -1000;

    /// <summary>
    /// 排序属性
    /// </summary>
    public int Order => FilterOrder;

    /// <summary>
    /// 是否是可重复使用的
    /// </summary>
    public static bool IsReusable => true;

    /// <summary>
    /// 拦截请求
    /// </summary>
    /// <param name="context">动作方法上下文</param>
    /// <param name="next">中间件委托</param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 排除 WebSocket 请求处理
        if (context.HttpContext.IsWebSocketRequest())
        {
            await next();
            return;
        }

        // 获取控制器/方法信息
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

        // 跳过验证类型
        var nonValidationAttributeType = typeof(NonValidationAttribute);
        var method = actionDescriptor.MethodInfo;

        // 获取验证状态
        var modelState = context.ModelState;

        // 如果参数数量为 0 或贴了 [NonValidation] 特性 或所在类型贴了 [NonValidation] 特性或验证成功或已经设置了结果，则跳过验证
        if (actionDescriptor.Parameters.Count == 0 ||
            method.IsDefined(nonValidationAttributeType, true) ||
            method.DeclaringType.IsDefined(nonValidationAttributeType, true) ||
            modelState.IsValid ||
            method.DeclaringType.Assembly.GetName().Name.StartsWith("Microsoft.AspNetCore.OData") ||
            context.Result != null)
        {
            await CallUnHandleResult(context, next, actionDescriptor, method);
            return;
        }

        // 处理执行前验证信息
        var handledResult = HandleValidation(context, method, actionDescriptor, modelState);

        // 处理 Mvc 未处理结果情况
        if (!handledResult)
        {
            await CallUnHandleResult(context, next, actionDescriptor, method);
        }
    }

    /// <summary>
    /// 调用未处理的结果类型
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <param name="actionDescriptor"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    private async Task CallUnHandleResult(ActionExecutingContext context, ActionExecutionDelegate next, ControllerActionDescriptor actionDescriptor, MethodInfo method)
    {
        // 处理执行后验证信息
        var resultContext = await next();

        // 如果异常不为空且属于友好验证异常
        if (resultContext.Exception != null && resultContext.Exception is AppFriendlyException friendlyException && friendlyException.ValidationException)
        {
            // 存储验证执行结果
            context.HttpContext.Items[nameof(DataValidationFilter) + nameof(AppFriendlyException)] = resultContext;

            // 处理验证信息
            _ = HandleValidation(context, method, actionDescriptor, friendlyException.ErrorMessage, resultContext, friendlyException);
        }
    }

    /// <summary>
    /// 内部处理异常
    /// </summary>
    /// <param name="context"></param>
    /// <param name="method"></param>
    /// <param name="actionDescriptor"></param>
    /// <param name="errors"></param>
    /// <param name="resultContext"></param>
    /// <param name="friendlyException"></param>
    /// <returns>返回 false 表示结果没有处理</returns>
    private bool HandleValidation(ActionExecutingContext context, MethodInfo method, ControllerActionDescriptor actionDescriptor, object errors, ActionExecutedContext resultContext = default, AppFriendlyException friendlyException = default)
    {
        dynamic finalContext = resultContext != null ? resultContext : context;

        // 解析验证消息
        var validationMetadata = ValidatorContext.GetValidationMetadata(errors);
        validationMetadata.ErrorCode = friendlyException?.ErrorCode;
        validationMetadata.OriginErrorCode = friendlyException?.OriginErrorCode;
        validationMetadata.StatusCode = friendlyException?.StatusCode;
        validationMetadata.Data = friendlyException?.Data;

        // 存储验证信息
        context.HttpContext.Items[nameof(DataValidationFilter) + nameof(ValidationMetadata)] = validationMetadata;

        // 判断是否跳过规范化结果，如果跳过，返回 400 BadRequestResult
        if (UnifyContext.CheckFailedNonUnify(actionDescriptor.MethodInfo, out var unifyResult))
        {
            // WebAPI 情况
            if (Penetrates.IsApiController(method.DeclaringType))
            {
                // 如果不启用 SuppressModelStateInvalidFilter，则跳过，理应手动验证
                if (!_apiBehaviorOptions.SuppressModelStateInvalidFilter)
                {
                    finalContext.Result = _apiBehaviorOptions.InvalidModelStateResponseFactory(context);
                }
                else
                {
                    // 返回 JsonResult
                    finalContext.Result = new JsonResult(validationMetadata.ValidationResult)
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
            }
            else
            {
                // 返回自定义错误页面
                finalContext.Result = new BadPageResult(StatusCodes.Status400BadRequest)
                {
                    Code = validationMetadata.Message
                };
            }
        }
        else
        {
            // 判断是否支持 MVC 规范化处理，一旦启用，则自动调用规范化提供器进行操作，这里返回 false 表示没有处理结果
            if (!UnifyContext.CheckSupportMvcController(context.HttpContext, actionDescriptor, out _)
                || UnifyContext.CheckHttpContextNonUnify(context.HttpContext)) return false;

            finalContext.Result = unifyResult.OnValidateFailed(context, validationMetadata);
        }

        // 打印验证失败信息
        App.PrintToMiniProfiler("validation", "Failed", $"Validation Failed:\r\n\r\n{validationMetadata.Message}", true);

        return true;
    }
}