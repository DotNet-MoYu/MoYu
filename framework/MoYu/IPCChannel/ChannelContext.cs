﻿// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using MoYu.FriendlyException;
using System.Threading.Channels;

namespace MoYu.IPCChannel;

/// <summary>
/// 进程管道内通信上下文
/// </summary>
/// <typeparam name="TMessage"></typeparam>
/// <typeparam name="THandler"></typeparam>
/// <remarks>后续将通过 MemoryMapperFile 共享内存实现 IPC 通信：https://docs.microsoft.com/zh-cn/dotnet/api/system.io.memorymappedfiles.memorymappedfile?view=net-5.0 </remarks>
[SuppressSniffer]
public sealed class ChannelContext<TMessage, THandler>
    where THandler : ChannelHandler<TMessage>
{
    /// <summary>
    /// 通过懒加载创建无限容量通道
    /// </summary>
    private static readonly Lazy<Channel<TMessage>> _unBoundedChannel = new(() =>
    {
        var channel = Channel.CreateUnbounded<TMessage>(new UnboundedChannelOptions
        {
            SingleReader = false,   // 允许多个管道读写，提供管道吞吐量（无序操作）
            SingleWriter = false
        });

        StartReader(channel);
        return channel;
    });

    /// <summary>
    /// 通过懒加载创建有限容量通道
    /// </summary>
    /// <remarks>默认容量为 1000</remarks>
    private static readonly Lazy<Channel<TMessage>> _boundedChannel = new(() =>
    {
        var channel = Channel.CreateBounded<TMessage>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,   // 允许多个管道读写，提供管道吞吐量（无序操作）
            SingleWriter = false
        });

        StartReader(channel);
        return channel;
    });

    /// <summary>
    /// 无限容量通道
    /// </summary>
    public static Channel<TMessage> UnBoundedChannel => _unBoundedChannel.Value;

    /// <summary>
    /// 有限容量通道
    /// </summary>
    public static Channel<TMessage> BoundedChannel => _boundedChannel.Value;

    /// <summary>
    /// 私有构造函数
    /// </summary>
    private ChannelContext()
    {
    }

    /// <summary>
    /// 创建一个读取器
    /// </summary>
    /// <param name="channel"></param>
    private static void StartReader(Channel<TMessage> channel)
    {
        var reader = channel.Reader;

        // 创建长时间线程管道读取器
        _ = Task.Factory.StartNew(async () =>
          {
              while (await reader.WaitToReadAsync())
              {
                  if (!reader.TryRead(out var message)) continue;

                  // 并行执行（非等待）
                  await Retry.InvokeAsync(async () => await Activator.CreateInstance<THandler>().InvokeAsync(message), 3, 1000, finalThrow: false);
              }
          }, TaskCreationOptions.LongRunning);
    }
}