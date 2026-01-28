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

using MoYu.TimeCrontab;

namespace MoYu.TaskQueue;

/// <summary>
/// 任务包装器
/// </summary>
[SuppressSniffer]
public sealed class TaskWrapper
{
    /// <summary>
    /// 任务处理委托
    /// </summary>
    internal Func<IServiceProvider, CancellationToken, ValueTask> Handler
    {
        get;
        set
        {
            field = async (serviceProvider, cancellationToken) =>
            {
                if (Delay > 0)
                {
                    // 配置是否设置了延迟执行后立即执行一次
                    if (RunOnceIfDelaySet)
                    {
                        await value(serviceProvider, cancellationToken);
                    }

                    await Task.Delay(Delay, cancellationToken);
                }

                await value(serviceProvider, cancellationToken);
            };
        }
    }

    /// <summary>
    /// 任务通道
    /// </summary>
    internal string Channel { get; set; } = string.Empty;

    /// <summary>
    /// 任务 ID
    /// </summary>
    internal object TaskId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 是否采用并行执行
    /// </summary>
    internal bool? Concurrent { get; set; }

    /// <summary>
    /// 延迟时间（毫秒）
    /// </summary>
    internal int Delay { get; set; } = 0;

    /// <summary>
    /// 配置是否设置了延迟执行后立即执行一次
    /// </summary>
    internal bool RunOnceIfDelaySet { get; set; }

    /// <summary>
    /// 是否禁止重试
    /// </summary>
    internal bool DisableRetry { get; set; }

    /// <summary>
    /// 设置任务通道
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    public TaskWrapper WithChannel(string channel)
    {
        Channel = channel ?? string.Empty;

        return this;
    }

    /// <summary>
    /// 设置任务 ID
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    public TaskWrapper WithTaskId(object taskId)
    {
        TaskId = taskId ?? Guid.NewGuid();

        return this;
    }

    /// <summary>
    /// 设置是否采用并行执行
    /// </summary>
    /// <param name="concurrent"></param>
    /// <returns></returns>
    public TaskWrapper WithConcurrent(bool? concurrent)
    {
        Concurrent = concurrent;

        return this;
    }

    /// <summary>
    /// 设置延迟时间（毫秒）
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    public TaskWrapper WithDelay(int delay)
    {
        Delay = delay;

        return this;
    }

    /// <summary>
    /// 设置延迟时间（毫秒）
    /// </summary>
    /// <param name="cronExpression">Cron 表达式</param>
    /// <param name="format"><see cref="CronStringFormat"/></param>
    /// <returns></returns>
    public TaskWrapper WithDelay(string cronExpression, CronStringFormat format = CronStringFormat.Default)
    {
        return WithDelay((int)Crontab.Parse(cronExpression, format).GetSleepMilliseconds(DateTime.Now));
    }

    /// <summary>
    /// 设置是否延迟执行后立即执行一次
    /// </summary>
    /// <param name="runOnceIfDelaySet"></param>
    /// <returns></returns>
    public TaskWrapper WithRunOnceIfDelaySet(bool runOnceIfDelaySet)
    {
        RunOnceIfDelaySet = runOnceIfDelaySet;

        return this;
    }

    /// <summary>
    /// 设置是否禁止重试
    /// </summary>
    /// <param name="disableRetry"></param>
    /// <returns></returns>
    public TaskWrapper WithDisableRetry(bool disableRetry)
    {
        DisableRetry = disableRetry;

        return this;
    }
}