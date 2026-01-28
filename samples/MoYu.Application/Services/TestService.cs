namespace MoYu.Application.Services;

public class TestService : ITestService, ITransient
{
    public string GetName()
    {
        return "MoYu";
    }
}