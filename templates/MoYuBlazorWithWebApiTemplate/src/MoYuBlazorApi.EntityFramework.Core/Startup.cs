using MoYu;
using Microsoft.Extensions.DependencyInjection;

namespace MoYuBlazorApi.EntityFramework.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDatabaseAccessor(options =>
        {
            options.AddDbPool<DefaultDbContext>();
        }, "MoYuBlazorApi.Database.Migrations");
    }
}
