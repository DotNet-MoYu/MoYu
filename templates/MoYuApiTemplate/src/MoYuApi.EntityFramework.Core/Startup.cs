using MoYu;
using Microsoft.Extensions.DependencyInjection;

namespace MoYuApi.EntityFramework.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDatabaseAccessor(options =>
        {
            options.AddDbPool<DefaultDbContext>();
        }, "MoYuApi.Database.Migrations");
    }
}
