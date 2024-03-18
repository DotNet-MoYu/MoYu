using MoYu;
using SqlSugar;
using System.Collections.Generic;

namespace MoYuRazorApi.Core;

/// <summary>
/// ���ݿ������Ķ���
/// </summary>
public static class DbContext
{
    /// <summary>
    /// SqlSugar ���ݿ�ʵ��
    /// </summary>
    public static readonly SqlSugarScope Instance = new(
        // ��ȡ appsettings.json �е� ConnectionConfigs ���ýڵ�
        App.GetConfig<List<ConnectionConfig>>("ConnectionConfigs")
        , db =>
        {
            // ��������ȫ���¼�����������ִ�� SQL
        });
}
