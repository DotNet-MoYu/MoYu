// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using MoYu.FriendlyException;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Logging;

namespace MoYu.TaskQueue;

/// <summary>
/// 任务队列后台主机服务
/// </summary>
/// <remarks>用于长时间监听任务项入队后进行出队调用</remarks>
internal sealed class TaskQueueHostedService : BackgroundService
{
    /// <summary>
    /// 避免由 CLR 的终结器捕获该异常从而终止应用程序，让所有未觉察异常被觉察
    /// </summary>
    internal event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

    /// <summary>
    /// 日志对象
    /// </summary>
    private readonly ILogger<TaskQueueService> _logger;

    /// <summary>
    /// 服务提供器
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 后台任务队列
    /// </summary>
    private readonly ITaskQueue _taskQueue;

    /// <summary>
    /// 是否采用并行执行
    /// </summary>
    private readonly bool _concurrent;

    /// <summary>
    /// 重试次数
    /// </summary>
    private readonly int _numRetries;

    /// <summary>
    /// 重试间隔
    /// </summary>
    private readonly int _retryTimeout;

    /// <summary>
    /// 长时间运行的后台任务
    /// </summary>
    /// <remarks>解决局部设置执行策略问题</remarks>
    private Task _processQueueTask;

    /// <summary>
    /// 同步任务队列（线程安全）
    /// </summary>
    private readonly BlockingCollection<TaskWrapper> _syncTaskWrapperQueue = new(3000);

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志对象</param>
    /// <param name="serviceProvider">服务提供器</param>
    /// <param name="taskQueue">后台任务队列</param>
    /// <param name="concurrent">是否采用并行执行</param>
    /// <param name="numRetries">重试次数</param>
    /// <param name="retryTimeout">重试间隔</param>
    public TaskQueueHostedService(ILogger<TaskQueueService> logger
        , IServiceProvider serviceProvider
        , ITaskQueue taskQueue
        , bool concurrent
        , int numRetries
        , int retryTimeout)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _taskQueue = taskQueue;
        _concurrent = concurrent;
        _numRetries = numRetries;
        _retryTimeout = retryTimeout;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        // 创建长时间运行的后台任务，并将同步任务队列中任务进行遍历消费
        _processQueueTask = Task.Factory.StartNew(async state => await ((TaskQueueHostedService)state).ProcessQueueAsync(cancellationToken)
            , this, TaskCreationOptions.LongRunning);

        return base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// 执行后台任务
    /// </summary>
    /// <param name="stoppingToken">后台主机服务停止时取消任务 Token</param>
    /// <returns>Task</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TaskQueue hosted service is running.");

        // 注册后台主机服务停止监听
        stoppingToken.Register(() =>
            _logger.LogDebug($"TaskQueue hosted service is stopping."));

        // 监听服务是否取消
        while (!stoppingToken.IsCancellationRequested)
        {
            // 执行具体任务
            await BackgroundProcessing(stoppingToken);
        }

        _logger.LogCritical($"TaskQueue hosted service is stopped.");
    }

    /// <summary>
    /// 后台调用处理程序
    /// </summary>
    /// <param name="stoppingToken">后台主机服务停止时取消任务 Token</param>
    /// <returns><see cref="Task"/></returns>
    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        // 出队
        var taskWrapper = await _taskQueue.DequeueAsync(stoppingToken);

        // 获取任务执行策略
        var concurrent = taskWrapper.Concurrent == null
            ? _concurrent
            : (bool)taskWrapper.Concurrent;

        // 并行执行
        if (concurrent)
        {
            Parallel.For(0, 1, async _ =>
            {
                await DequeueHandleAsync(taskWrapper, stoppingToken);
            });
        }
        // 依次出队执行：https://gitee.com/dotnetchina/MoYu/issues/I8VXFV
        else
        {
            // 只有队列可持续入队才写入
            if (!_syncTaskWrapperQueue.IsAddingCompleted)
            {
                try
                {
                    _syncTaskWrapperQueue.Add(taskWrapper, stoppingToken);
                }
                catch (InvalidOperationException) { }
                catch { }
            }
        }
    }

    /// <summary>
    /// 同步队列出队
    /// </summary>
    /// <param name="stoppingToken">后台主机服务停止时取消任务 Token</param>
    /// <returns><see cref="Task"/></returns>
    private async Task ProcessQueueAsync(CancellationToken stoppingToken)
    {
        foreach (var taskWrapper in _syncTaskWrapperQueue.GetConsumingEnumerable(stoppingToken))
        {
            await DequeueHandleAsync(taskWrapper, stoppingToken);
        }
    }

    /// <summary>
    /// 出队调用处理程序
    /// </summary>
    /// <param name="taskWrapper">任务包装器</param>
    /// <param name="stoppingToken">后台主机服务停止时取消任务 Token</param>
    /// <returns><see cref="Task"/></returns>
    private async Task DequeueHandleAsync(TaskWrapper taskWrapper, CancellationToken stoppingToken)
    {
        try
        {
            // 调用任务处理程序并配置出错执行重试
            await Retry.InvokeAsync(async () =>
            {
                // 调用任务处理委托
                await taskWrapper.Handler(_serviceProvider, stoppingToken);
            }
            , _numRetries
            , _retryTimeout
            , retryAction: (total, times) =>
            {
                // 输出重试日志
                _logger.LogWarning("Retrying {times}/{total} times for {TaskHandler}", times, total, taskWrapper.Handler?.ToString());
            });

            // 触发任务队列事件
            _taskQueue.InvokeEvents(new(taskWrapper.TaskId, taskWrapper.Channel, true));
        }
        catch (Exception ex)
        {
            // 输出异常日志
            _logger.LogError(ex, "Error occurred executing in {TaskHandler}.", taskWrapper.Handler?.ToString());

            // 捕获 Task 任务异常信息并统计所有异常
            if (UnobservedTaskException != default)
            {
                var args = new UnobservedTaskExceptionEventArgs(
                    ex as AggregateException ?? new AggregateException(ex));

                UnobservedTaskException.Invoke(this, args);
            }

            // 触发任务队列事件
            _taskQueue.InvokeEvents(new(taskWrapper.TaskId, taskWrapper.Channel, false)
            {
                Exception = ex
            });
        }
        finally
        {
            taskWrapper = null;
        }
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        base.Dispose();

        // 标记同步任务队列停止写入
        _syncTaskWrapperQueue.CompleteAdding();

        try
        {
            // 设置 1.5秒的缓冲时间，避免还有同步任务队列没有完成消费
            _processQueueTask?.Wait(1500);
        }
        catch (TaskCanceledException) { }
        catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException) { }
        catch { }
    }
}