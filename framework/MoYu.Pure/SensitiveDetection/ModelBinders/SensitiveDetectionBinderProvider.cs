// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System.ComponentModel.DataAnnotations;

namespace MoYu.SensitiveDetection;

/// <summary>
/// 脱敏词汇（脱敏）提供器模型绑定
/// </summary>
/// <remarks>用于替换脱敏词汇</remarks>
[SuppressSniffer]
public class SensitiveDetectionBinderProvider : IModelBinderProvider
{
    /// <summary>
    /// 返回自定义绑定器
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // 只处理字符串类型值
        if (context.Metadata.ModelType == typeof(string) && context.Metadata is DefaultModelMetadata actMetadata)
        {
            // 判断是否定义 [SensitiveDetection] 特性且 Transfer 不为空
            if (actMetadata.Attributes.ParameterAttributes != null
                && actMetadata.Attributes.ParameterAttributes.Count > 0
                && actMetadata.Attributes.ParameterAttributes.FirstOrDefault(u => u.GetType() == typeof(SensitiveDetectionAttribute))
                is SensitiveDetectionAttribute sensitiveDetectionAttribute && sensitiveDetectionAttribute.Transfer != default)
            {
                return new BinderTypeModelBinder(typeof(SensitiveDetectionBinder));
            }
        }

        return null;
    }
}