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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Net.Mime;
using System.Text;

namespace MoYu.AspNetCore.Formatters;

/// <summary>
///     从请求正文中读取 <c>text/plain</c> 内容
/// </summary>
/// <remarks>参考文献：https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/Formatters/SystemTextJsonInputFormatter.cs。</remarks>
public class TextPlainInputFormatter : TextInputFormatter, IInputFormatterExceptionPolicy
{
    /// <inheritdoc cref="TextPlainInputFormatter" />
    public TextPlainInputFormatter()
    {
        SupportedEncodings.Add(UTF8EncodingWithoutBOM);
        SupportedEncodings.Add(UTF16EncodingLittleEndian);

        SupportedMediaTypes.Add(MediaTypeNames.Text.Plain);
    }

    /// <inheritdoc />
    public InputFormatterExceptionPolicy ExceptionPolicy => InputFormatterExceptionPolicy.AllExceptions;

    /// <inheritdoc />
    public sealed override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context,
        Encoding encoding)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(encoding);

        // 获取 HttpContext 实例
        var httpContext = context.HttpContext;

        // 获取输入的流
        var (inputStream, usesTranscodingStream) = GetInputStream(httpContext, encoding);

        string? data;

        try
        {
            // 读取流中的字符串
            using var streamReader = new StreamReader(inputStream);
            data = await streamReader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            context.ModelState.TryAddModelError(string.Empty, ex, context.Metadata);

            return await InputFormatterResult.FailureAsync();
        }
        finally
        {
            if (usesTranscodingStream)
            {
                await inputStream.DisposeAsync();
            }
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (data is null && !context.TreatEmptyInputAsDefaultValue)
        {
            return await InputFormatterResult.NoValueAsync();
        }

        return await InputFormatterResult.SuccessAsync(data);
    }

    /// <summary>
    ///     获取输入的流
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="encoding">
    ///     <see cref="Encoding" />
    /// </param>
    /// <returns>
    ///     <see cref="Tuple{T1, T2}" />
    /// </returns>
    internal static (Stream inputStream, bool usesTranscodingStream) GetInputStream(HttpContext httpContext,
        Encoding encoding)
    {
        if (encoding.CodePage == Encoding.UTF8.CodePage)
        {
            return (httpContext.Request.Body, false);
        }

        var inputStream = Encoding.CreateTranscodingStream(httpContext.Request.Body, encoding, Encoding.UTF8, true);

        return (inputStream, true);
    }
}