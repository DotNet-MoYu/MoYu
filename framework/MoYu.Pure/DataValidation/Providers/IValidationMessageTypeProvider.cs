﻿// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

namespace MoYu.DataValidation;

/// <summary>
/// 验证消息类型提供器
/// </summary>
public interface IValidationMessageTypeProvider
{
    /// <summary>
    /// 验证消息类型定义
    /// </summary>
    Type[] Definitions { get; }
}