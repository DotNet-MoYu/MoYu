// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using MoYu.DataValidation;
using MoYu.FriendlyException;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MoYu.UnifyResult;

/// <summary>
/// 规范化结果提供器
/// </summary>
public interface IUnifyResultProvider
{
    /// <summary>
    /// JWT 授权异常返回值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    IActionResult OnAuthorizeException(DefaultHttpContext context, ExceptionMetadata metadata);

    /// <summary>
    /// 异常返回值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    IActionResult OnException(ExceptionContext context, ExceptionMetadata metadata);

    /// <summary>
    /// 成功返回值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    IActionResult OnSucceeded(ActionExecutedContext context, object data);

    /// <summary>
    /// 验证失败返回值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    IActionResult OnValidateFailed(ActionExecutingContext context, ValidationMetadata metadata);

    /// <summary>
    /// 拦截返回状态码
    /// </summary>
    /// <param name="context"></param>
    /// <param name="statusCode"></param>
    /// <param name="unifyResultSettings"></param>
    /// <returns></returns>
    Task OnResponseStatusCodes(HttpContext context, int statusCode, UnifyResultSettingsOptions unifyResultSettings = default);
}