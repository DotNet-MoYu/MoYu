using MoYu;
using System.Reflection;

namespace MoYuBlazorApi.Web.Entry;

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
            "MoYuBlazorApi.Application",
            "MoYuBlazorApi.Core",
            "MoYuBlazorApi.Web.Core"
        };
    }
}
