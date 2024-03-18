using MoYu;
using Microsoft.Extensions.DependencyInjection;

namespace MoYuApp.EntityFramework.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDatabaseAccessor(options =>
        {
            options.AddDbPool<DefaultDbContext>();
        }, "MoYuApp.Database.Migrations");
    }
}
