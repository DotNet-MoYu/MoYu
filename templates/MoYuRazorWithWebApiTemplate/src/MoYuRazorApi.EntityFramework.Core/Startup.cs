using MoYu;
using Microsoft.Extensions.DependencyInjection;

namespace MoYuRazorApi.EntityFramework.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDatabaseAccessor(options =>
        {
            options.AddDbPool<DefaultDbContext>();
        }, "MoYuRazorApi.Database.Migrations");
    }
}
