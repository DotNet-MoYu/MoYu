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

using MoYu.ClayObject;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MoYu.JsonSerialization;

/// <summary>
/// Clay 类型序列化
/// </summary>
[SuppressSniffer]
public class SystemTextJsonClayJsonConverter : JsonConverter<Clay>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SystemTextJsonClayJsonConverter()
        : this(true)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="toCamelCaseKey"></param>
    public SystemTextJsonClayJsonConverter(bool toCamelCaseKey)
    {
        ToCamelCaseKey = toCamelCaseKey;
    }

    /// <summary>
    /// 输出键小写
    /// </summary>
    public bool ToCamelCaseKey { get; set; } = true;

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override Clay Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Clay.Parse(reader.GetString());
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, Clay value, JsonSerializerOptions options)
    {
        var json = value.ToString();

        if (ToCamelCaseKey)
        {
            writer.WriteRawValue(ConvertKeysToCamelCase(JsonNode.Parse(json)).ToString());
        }
        else
        {
            writer.WriteRawValue(json);
        }
    }

    /// <summary>
    /// 转换 Key 为小写
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private static JsonNode ConvertKeysToCamelCase(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            var newObj = new JsonObject();
            foreach (var prop in obj)
            {
                var newKey = char.ToLower(prop.Key[0]) + prop.Key.Substring(1);
                newObj[newKey] = DeepCopy(ConvertKeysToCamelCase(prop.Value));
            }
            return newObj;
        }
        else if (node is JsonArray array)
        {
            var newArray = new JsonArray();
            foreach (var item in array)
            {
                newArray.Add(DeepCopy(ConvertKeysToCamelCase(item)));
            }
            return newArray;
        }
        return node;
    }

    private static JsonNode DeepCopy(JsonNode node)
    {
        return JsonSerializer.Deserialize<JsonNode>(node.ToJsonString());
    }
}