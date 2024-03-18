// 版权归百小僧及百签科技（广东）有限公司所有。
//
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoYu.DatabaseAccessor;

/// <summary>
/// 租户实体
/// </summary>
public class Tenant : IEntity<MultiTenantDbContextLocator>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public Tenant()
    {
        CreatedTime = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 租户Id
    /// </summary>
    [Key]
    [ScaffoldColumn(false)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual Guid TenantId { get; set; }

    /// <summary>
    /// 租户名
    /// </summary>
    [Required, MaxLength(128)]
    public virtual string Name { get; set; }

    /// <summary>
    /// 来源主机地址
    /// </summary>
    [MaxLength(256)]
    public virtual string Host { get; set; }

    /// <summary>
    /// 电子邮箱
    /// </summary>
    [EmailAddress, MaxLength(256)]
    public virtual string EmailAddress { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [Phone, MaxLength(32)]
    public virtual string PhoneNumber { get; set; }

    /// <summary>
    /// 架构名
    /// </summary>
    [MaxLength(32)]
    public virtual string Schema { get; set; }

    /// <summary>
    /// 数据库链接字符串
    /// </summary>
    [MaxLength(256)]
    public virtual string ConnectionString { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTimeOffset CreatedTime { get; set; }
}