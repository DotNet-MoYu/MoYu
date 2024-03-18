using MoYu;
using System.Reflection;

namespace MoYuRazorApi.Web.Entry;

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
            "MoYuRazorApi.Application",
            "MoYuRazorApi.Core",
            "MoYuRazorApi.EntityFramework.Core",
            "MoYuRazorApi.Web.Core"
        };
    }
}
