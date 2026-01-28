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

using MoYu.Shapeless;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MoYu.JsonSerialization;

/// <summary>
/// 解决 Clay 问题
/// </summary>
[SuppressSniffer]
public class NewtonsoftJsonClayJsonConverter : JsonConverter<Clay>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public NewtonsoftJsonClayJsonConverter()
        : this(true)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="toCamelCaseKey"></param>
    public NewtonsoftJsonClayJsonConverter(bool toCamelCaseKey)
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
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="hasExistingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override Clay ReadJson(JsonReader reader, Type objectType, Clay existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var value = JValue.ReadFrom(reader).Value<string>();
        return Clay.Parse(value);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, Clay value, JsonSerializer serializer)
    {
        var json = value.ToString();

        if (ToCamelCaseKey)
        {
            writer.WriteRawValue(ConvertKeysToCamelCase(JToken.Parse(json)).ToString());
        }
        else
        {
            writer.WriteRawValue(json);
        }
    }

    /// <summary>
    /// 转换 Key 为小写
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private static JToken ConvertKeysToCamelCase(JToken token)
    {
        if (token is JObject jObj)
        {
            var newJObject = new JObject();
            foreach (var prop in jObj.Properties())
            {
                var newKey = char.ToLower(prop.Name[0]) + prop.Name.Substring(1);
                newJObject[newKey] = ConvertKeysToCamelCase(prop.Value);
            }
            return newJObject;
        }
        else if (token is JArray jArray)
        {
            var newArray = new JArray();
            foreach (var item in jArray)
            {
                newArray.Add(ConvertKeysToCamelCase(item));
            }
            return newArray;
        }
        return token;
    }
}