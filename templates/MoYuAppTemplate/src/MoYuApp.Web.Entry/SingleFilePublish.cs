using MoYu;
using System.Reflection;

namespace MoYuApp.Web.Entry;

public class SingleFilePublish : ISingleFilePublish
{
    public Assembly[] IncludeAssemblies()
    {
        return Array.Empty<Assembly>();
    }

    public string[] IncludeAssemblyNames()
    {
        return new[]
        {
            "MoYuApp.Application",
            "MoYuApp.Core",
            "MoYuApp.EntityFramework.Core",
            "MoYuApp.Web.Core"
        };
    }
}
