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

using System.Threading.Channels;

namespace MoYu.TaskQueue;

/// <summary>
/// 任务队列默认实现
/// </summary>
internal sealed partial class TaskQueue : ITaskQueue
{
    /// <summary>
    /// 任务委托执行事件
    /// </summary>
    public event EventHandler<TaskHandlerEventArgs> OnExecuted;

    /// <summary>
    /// 队列通道
    /// </summary>
    private readonly Channel<TaskWrapper> _queue;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="capacity">队列通道默认容量，超过该容量进入等待</param>
    public TaskQueue(int capacity)
    {
        // 配置通道，设置超出默认容量后进入等待
        var boundedChannelOptions = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };

        // 创建有限容量通道
        _queue = Channel.CreateBounded<TaskWrapper>(boundedChannelOptions);
    }

    /// <summary>
    /// 任务项出队
    /// </summary>
    /// <param name="cancellationToken">取消任务 Token</param>
    /// <returns><see cref="ValueTask"/></returns>
    public async ValueTask<TaskWrapper> DequeueAsync(CancellationToken cancellationToken)
    {
        try
        {
            // 读取管道队列
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
        // 正常取消，服务停止时触发
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public object Enqueue(Action<IServiceProvider> taskHandler, Action<TaskWrapper> configure = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(taskHandler);

        return EnqueueAsync((serviceProvider, token) =>
        {
            taskHandler(serviceProvider);
            return ValueTask.CompletedTask;
        }, configure)
        .AsTask().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async ValueTask<object> EnqueueAsync(Func<IServiceProvider, CancellationToken, ValueTask> taskHandler, Action<TaskWrapper> configure = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(taskHandler);

        // 调用自定义配置委托
        var taskWrapper = new TaskWrapper();
        configure?.Invoke(taskWrapper);
        taskWrapper.Handler = taskHandler;  // 确保放置最后设置

        // 写入管道队列
        await _queue.Writer.WriteAsync(taskWrapper);

        return taskWrapper.TaskId;
    }

    /// <summary>
    /// 触发任务队列事件
    /// </summary>
    /// <param name="args">事件参数</param>
    public void InvokeEvents(TaskHandlerEventArgs args)
    {
        try
        {
            OnExecuted?.Invoke(this, args);
        }
        catch { }
    }
}