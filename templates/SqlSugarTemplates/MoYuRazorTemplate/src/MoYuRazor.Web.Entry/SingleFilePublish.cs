using MoYu;
using System.Reflection;

namespace MoYuRazor.Web.Entry;

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
            "MoYuRazor.Application",
            "MoYuRazor.Core",
            "MoYuRazor.Web.Core"
        };
    }
}
