namespace MoYuBlazorApi.Application;

public class SystemService : ISystemService, IDynamicApiController, ITransient
{
    public string GetDescription()
    {
        return "�� .NET �������򵥣���ͨ�ã������С�";
    }
}
