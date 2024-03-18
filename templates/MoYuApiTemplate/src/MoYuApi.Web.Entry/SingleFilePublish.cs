using MoYu;
using System.Reflection;

namespace MoYuApi.Web.Entry;

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
            "MoYuApi.Application",
            "MoYuApi.Core",
            "MoYuApi.EntityFramework.Core",
            "MoYuApi.Web.Core"
        };
    }
}
