using Microsoft.Extensions.DependencyInjection;

namespace MoYu.Application;

[AppStartup(900)]
public sealed class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddConfigurableOptions<AppInfoOptions>();
    }
}