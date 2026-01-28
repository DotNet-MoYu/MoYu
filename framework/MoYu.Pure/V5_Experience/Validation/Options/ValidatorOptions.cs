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

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace MoYu.Validation;

/// <summary>
///     验证器选项
/// </summary>
public sealed class ValidatorOptions : INotifyPropertyChanged
{
    /// <summary>
    ///     是否禁用属性验证特性验证
    /// </summary>
    /// <remarks>默认值为：<c>false</c>，即启用。</remarks>
    public bool SuppressAttributeValidation { get; set => SetField(ref field, value); }

    /// <summary>
    ///     是否验证所有属性的验证特性
    /// </summary>
    /// <remarks>
    ///     该属性用于控制是否执行属性级别的验证逻辑，默认值为 <c>true</c>。
    ///     若设置为 <c>true</c>，则会同时验证所有属性以及 <see cref="IValidatableObject.Validate" /> 方法；
    ///     若设置为 <c>false</c>，则仅验证 <see cref="IValidatableObject.Validate" /> 方法。
    /// </remarks>
    public bool ValidateAllProperties { get; set => SetField(ref field, value); } = true;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     触发属性变更事件
    /// </summary>
    /// <param name="propertyName">属性名称</param>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    ///     设置字段值
    /// </summary>
    /// <param name="field">属性关联的字段</param>
    /// <param name="value">已更改属性的值</param>
    /// <param name="propertyName">已更改属性的名称</param>
    /// <typeparam name="T">属性类型</typeparam>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        // 检查值是否发生变更
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;

        // 触发属性变更事件
        OnPropertyChanged(propertyName);

        return true;
    }
}