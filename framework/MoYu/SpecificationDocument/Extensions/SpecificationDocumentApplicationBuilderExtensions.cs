// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using MoYu;
using MoYu.SpecificationDocument;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 规范化文档中间件拓展
/// </summary>
[SuppressSniffer]
public static class SpecificationDocumentApplicationBuilderExtensions
{
    /// <summary>
    /// 添加规范化文档中间件
    /// </summary>
    /// <param name="app"></param>
    /// <param name="routePrefix"></param>
    /// <param name="configureSwagger"></param>
    /// <param name="configureSwaggerUI"></param>
    /// <param name="withProxy">解决 Swagger 被代理问题</param>
    /// <returns></returns>
    public static IApplicationBuilder UseSpecificationDocuments(this IApplicationBuilder app
        , string routePrefix = default
        , Action<SwaggerOptions> configureSwagger = default
        , Action<SwaggerUIOptions> configureSwaggerUI = default
        , bool withProxy = false)
    {
        // 判断是否启用规范化文档
        if (App.Settings.InjectSpecificationDocument != true) return app;

        // 配置 Swagger 全局参数
        app.UseSwagger(options => SpecificationDocumentBuilder.Build(options, configureSwagger));

        // 配置 Swagger UI 参数
        app.UseSwaggerUI(options => SpecificationDocumentBuilder.BuildUI(options, routePrefix, configureSwaggerUI, withProxy));

        // 启用 MiniProfiler组件
        if (App.Settings.InjectMiniProfiler == true) app.UseMiniProfiler();

        return app;
    }
}