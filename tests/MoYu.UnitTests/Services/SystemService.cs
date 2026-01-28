using MoYu.DependencyInjection;

namespace MoYu.UnitTests;

public class SystemService : ISystemService, ITransient
{
    public string GetName() => "MoYu";
}