namespace MoYuBlazorApp.Application;

public class SystemService : ISystemService, ITransient
{
    public string GetDescription()
    {
        return "�� .NET �������򵥣���ͨ�ã������С�";
    }
}