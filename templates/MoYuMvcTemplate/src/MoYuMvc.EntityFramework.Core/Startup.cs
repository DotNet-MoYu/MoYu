using MoYu;
using Microsoft.Extensions.DependencyInjection;

namespace MoYuMvc.EntityFramework.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDatabaseAccessor(options =>
        {
            options.AddDbPool<DefaultDbContext>();
        }, "MoYuMvc.Database.Migrations");
    }
}
