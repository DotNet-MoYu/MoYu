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

namespace MoYu.Extensions;

/// <summary>
///     数值类型扩展类
/// </summary>
public static class NumberExtensions
{
    /// <summary>
    ///     根据指定的单位将字节数进行转换
    /// </summary>
    /// <param name="byteSize">字节数</param>
    /// <param name="unit">单位。可选值为：<c>B</c>, <c>KB</c>, <c>MB</c>, <c>GB</c>, <c>TB</c>, <c>PB</c>, <c>EB</c>。</param>
    /// <returns>
    ///     <see cref="double" />
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static double ToSizeUnits(this double byteSize, string unit)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);

        // 非负检查
        if (byteSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteSize),
                $"The `{nameof(byteSize)}` must be non-negative.");
        }

        return unit.ToUpperInvariant() switch
        {
            "B" => byteSize,
            "KB" => byteSize / 1024.0,
            "MB" => byteSize / (1024.0 * 1024),
            "GB" => byteSize / (1024.0 * 1024 * 1024),
            "TB" => byteSize / (1024.0 * 1024 * 1024 * 1024),
            "PB" => byteSize / (1024.0 * 1024 * 1024 * 1024 * 1024),
            "EB" => byteSize / (1024.0 * 1024 * 1024 * 1024 * 1024 * 1024),
            _ => throw new ArgumentOutOfRangeException(nameof(unit), $"Unsupported `{unit}` unit.")
        };
    }

    /// <summary>
    ///     根据指定的单位将字节数进行转换
    /// </summary>
    /// <param name="byteSize">字节数</param>
    /// <param name="unit">单位。可选值为：<c>B</c>, <c>KB</c>, <c>MB</c>, <c>GB</c>, <c>TB</c>, <c>PB</c>, <c>EB</c>。</param>
    /// <returns>
    ///     <see cref="double" />
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static double ToSizeUnits(this long byteSize, string unit) => ((double)byteSize).ToSizeUnits(unit);

    /// <summary>
    ///     将毫秒数格式化为更直观的时间单位字符串（如 ms, s, min, h, d, y）
    /// </summary>
    /// <param name="millisecond">时间间隔的毫秒数</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static string FormatDuration(this long millisecond)
    {
        while (true)
        {
            // 如果时间小于 1000 毫秒（即 1 秒），直接以毫秒为单位返回
            if (millisecond < 1000)
            {
                return $"{millisecond}ms";
            }

            // 将毫秒转换为秒
            var seconds = millisecond / 1000.0;
            if (seconds < 60)
            {
                var val = Math.Round(seconds * 10) / 10;
                if (!(val >= 60))
                {
                    return val % 1 == 0 ? $"{val:F0}s" : $"{val:F1}s";
                }

                millisecond = (long)(val * 1000);
                continue;
            }

            // 将秒转换为分钟
            var minutes = seconds / 60;
            if (minutes < 60)
            {
                var val = Math.Round(minutes * 10) / 10;
                if (!(val >= 60))
                {
                    return val % 1 == 0 ? $"{val:F0}min" : $"{val:F1}min";
                }

                millisecond = (long)(val * 60 * 1000);
                continue;
            }

            // 将分钟转换为小时
            var hours = minutes / 60;
            if (hours < 24)
            {
                var val = Math.Round(hours * 10) / 10;
                if (!(val >= 24))
                {
                    return val % 1 == 0 ? $"{val:F0}h" : $"{val:F1}h";
                }

                millisecond = (long)(val * 60 * 60 * 1000);
                continue;
            }

            // 将小时转换为天
            var days = hours / 24;
            if (days < 365)
            {
                var val = Math.Round(days * 10) / 10;
                if (!(val >= 365))
                {
                    return val % 1 == 0 ? $"{val:F0}d" : $"{val:F1}d";
                }

                millisecond = (long)(val * 24 * 60 * 60 * 1000);
                continue;
            }

            // 将天数转换为年
            var years = days / 365;
            var finalVal = Math.Round(years * 10) / 10;
            return finalVal % 1 == 0 ? $"{finalVal:F0}y" : $"{finalVal:F1}y";
        }
    }

    /// <summary>
    ///     将毫秒数格式化为更直观的时间单位字符串（如 ms, s, min, h, d, y）
    /// </summary>
    /// <param name="millisecond">时间间隔的毫秒数</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static string FormatDuration(this double millisecond) => ((long)millisecond).FormatDuration();
}