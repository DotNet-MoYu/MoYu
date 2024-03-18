using MoYu.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;

namespace MoYuBlazorApi.EntityFramework.Core;

[AppDbContext("MoYuBlazorApi", DbProvider.Sqlite)]
public class DefaultDbContext : AppDbContext<DefaultDbContext>
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
    {
    }
}
