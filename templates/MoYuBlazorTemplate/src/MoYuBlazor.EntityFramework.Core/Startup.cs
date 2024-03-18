using MoYu;
using Microsoft.Extensions.DependencyInjection;

namespace MoYuBlazor.EntityFramework.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDatabaseAccessor(options =>
        {
            options.AddDbPool<DefaultDbContext>();
        }, "MoYuBlazor.Database.Migrations");
    }
}
