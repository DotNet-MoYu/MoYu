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

using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MoYu.Validation;

/// <summary>
///     提取验证选项元数据（<see cref="ValidationOptionsAttribute" />）的 MVC 验证器
/// </summary>
/// <remarks>
///     为实现了 <see cref="IValidatableObject" /> 接口的模型和 <see cref="ValidateWithAttribute{TValidator}" />
///     特性提供验证选项（如规则集）支持。
/// </remarks>
internal sealed class ValidationOptionsModelValidator : IModelValidator
{
    /// <inheritdoc />
    public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
    {
        // 检查 context.ActionContext.ActionDescriptor 是否是 ControllerActionDescriptor（MVC）
        if (context.ActionContext.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            yield break;
        }

        // 尝试获取验证数据上下文服务
        if (context.ActionContext.HttpContext.RequestServices.GetService<IValidationDataContext>() is not
            ValidationDataContext validationDataContext)
        {
            yield break;
        }

        // 检查是否已设置验证选项（避免重复提取）
        if (validationDataContext.HasValidationOptions())
        {
            yield break;
        }

        // 提取验证选项
        var validationOptionsMetadata = ExtractFromMethod(actionDescriptor.MethodInfo) ??
                                        ExtractFromDeclaredType(actionDescriptor.ControllerTypeInfo);

        // 设置当前验证选项（单次请求仅解析并设置一次，支持 null 值）
        validationDataContext.SetValidationOptions(validationOptionsMetadata);
    }

    /// <summary>
    ///     从操作方法（Action/Handler）提取验证选项
    /// </summary>
    /// <param name="methodInfo">
    ///     <see cref="MethodInfo" />
    /// </param>
    /// <returns>
    ///     <see cref="ValidationOptionsMetadata" />
    /// </returns>
    internal static ValidationOptionsMetadata? ExtractFromMethod(MethodInfo methodInfo) =>
        CreateMetadata(methodInfo.GetCustomAttribute<ValidationOptionsAttribute>(true));

    /// <summary>
    ///     从声明类（Controller/PageModel）提取验证选项
    /// </summary>
    /// <param name="typeInfo">
    ///     <see cref="TypeInfo" />
    /// </param>
    /// <returns>
    ///     <see cref="ValidationOptionsMetadata" />
    /// </returns>
    internal static ValidationOptionsMetadata? ExtractFromDeclaredType(TypeInfo typeInfo) =>
        CreateMetadata(typeInfo.GetCustomAttribute<ValidationOptionsAttribute>(true));

    /// <summary>
    ///     从 <see cref="ValidationOptionsAttribute" /> 中创建 <see cref="ValidationOptionsMetadata" /> 实例
    /// </summary>
    /// <param name="attribute">
    ///     <see cref="ValidationOptionsAttribute" />
    /// </param>
    /// <returns>
    ///     <see cref="ValidationOptionsMetadata" />
    /// </returns>
    internal static ValidationOptionsMetadata? CreateMetadata(ValidationOptionsAttribute? attribute) =>
        attribute is null ? null : new ValidationOptionsMetadata(attribute.RuleSets);
}