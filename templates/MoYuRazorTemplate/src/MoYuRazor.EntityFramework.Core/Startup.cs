using MoYu;
using Microsoft.Extensions.DependencyInjection;

namespace MoYuRazor.EntityFramework.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDatabaseAccessor(options =>
        {
            options.AddDbPool<DefaultDbContext>();
        }, "MoYuRazor.Database.Migrations");
    }
}
