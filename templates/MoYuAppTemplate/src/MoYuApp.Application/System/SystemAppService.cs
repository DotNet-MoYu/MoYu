namespace MoYuApp.Application;

/// <summary>
/// ϵͳ����ӿ�
/// </summary>
public class SystemAppService : IDynamicApiController
{
    private readonly ISystemService _systemService;
    public SystemAppService(ISystemService systemService)
    {
        _systemService = systemService;
    }

    /// <summary>
    /// ��ȡϵͳ����
    /// </summary>
    /// <returns></returns>
    public string GetDescription()
    {
        return _systemService.GetDescription();
    }
}
