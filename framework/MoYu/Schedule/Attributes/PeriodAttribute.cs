﻿// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

namespace MoYu.Schedule;

/// <summary>
/// 毫秒周期（间隔）作业触发器特性
/// </summary>
[SuppressSniffer, AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PeriodAttribute : TriggerAttribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="interval">间隔（毫秒）</param>
    public PeriodAttribute(long interval)
        : base(typeof(PeriodTrigger)
            , interval)
    {
    }
}