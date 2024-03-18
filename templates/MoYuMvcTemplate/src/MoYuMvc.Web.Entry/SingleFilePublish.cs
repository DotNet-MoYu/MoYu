using MoYu;
using System.Reflection;

namespace MoYuMvc.Web.Entry;

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
            "MoYuMvc.Application",
            "MoYuMvc.Core",
            "MoYuMvc.EntityFramework.Core",
            "MoYuMvc.Web.Core"
        };
    }
}
