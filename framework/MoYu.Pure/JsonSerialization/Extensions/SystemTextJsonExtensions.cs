// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using MoYu.JsonSerialization;
using System.Text.Json.Serialization;

namespace System.Text.Json;

/// <summary>
/// System.Text.Json 拓展
/// </summary>
[SuppressSniffer]
public static class SystemTextJsonExtensions
{
    /// <summary>
    /// 添加 DateTime/DateTime?/DateTimeOffset/DateTimeOffset? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="outputFormat"></param>
    /// <param name="localized">自动转换 DateTimeOffset 为当地时间</param>
    /// <returns></returns>
    public static IList<JsonConverter> AddDateTimeTypeConverters(this IList<JsonConverter> converters, string outputFormat = "yyyy-MM-dd HH:mm:ss", bool localized = false)
    {
        converters.Add(new SystemTextJsonDateTimeJsonConverter(outputFormat));
        converters.Add(new SystemTextJsonNullableDateTimeJsonConverter(outputFormat));

        converters.Add(new SystemTextJsonDateTimeOffsetJsonConverter(outputFormat, localized));
        converters.Add(new SystemTextJsonNullableDateTimeOffsetJsonConverter(outputFormat, localized));

        return converters;
    }

    /// <summary>
    /// 添加 long/long? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="overMaxLengthOf17">是否超过最大长度 17 再处理</param>
    /// <remarks></remarks>
    public static IList<JsonConverter> AddLongTypeConverters(this IList<JsonConverter> converters, bool overMaxLengthOf17 = false)
    {
        converters.Add(new SystemTextJsonLongToStringJsonConverter(overMaxLengthOf17));
        converters.Add(new SystemTextJsonNullableLongToStringJsonConverter(overMaxLengthOf17));

        return converters;
    }

    /// <summary>
    /// 添加 Clay 类型序列化处理
    /// </summary>
    /// <remarks></remarks>
    public static IList<JsonConverter> AddClayConverters(this IList<JsonConverter> converters)
    {
        converters.Add(new SystemTextJsonClayJsonConverter());

        return converters;
    }

    /// <summary>
    /// 添加 DateOnly/DateOnly? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <returns></returns>
    public static IList<JsonConverter> AddDateOnlyConverters(this IList<JsonConverter> converters)
    {
#if !NET5_0
        converters.Add(new SystemTextJsonDateOnlyJsonConverter());
        converters.Add(new SystemTextJsonNullableDateOnlyJsonConverter());
#endif
        return converters;
    }

    /// <summary>
    /// 添加 TimeOnly/TimeOnly? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <returns></returns>
    public static IList<JsonConverter> AddTimeOnlyConverters(this IList<JsonConverter> converters)
    {
#if !NET5_0
        converters.Add(new SystemTextJsonTimeOnlyJsonConverter());
        converters.Add(new SystemTextJsonNullableTimeOnlyJsonConverter());
#endif
        return converters;
    }
}