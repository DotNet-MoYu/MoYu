using MoYu;
using Microsoft.Extensions.DependencyInjection;

namespace MoYuBlazorApp.EntityFramework.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDatabaseAccessor(options =>
        {
            options.AddDbPool<DefaultDbContext>();
        }, "MoYuBlazorApp.Database.Migrations");
    }
}
