using MoYu;
using System.Reflection;

namespace MoYuBlazor.Web.Entry;

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
            "MoYuBlazor.Application",
            "MoYuBlazor.Core",
            "MoYuBlazor.EntityFramework.Core",
            "MoYuBlazor.Web.Core"
        };
    }
}
