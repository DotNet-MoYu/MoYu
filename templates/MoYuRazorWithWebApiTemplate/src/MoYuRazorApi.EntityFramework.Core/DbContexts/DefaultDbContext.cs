using MoYu.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;

namespace MoYuRazorApi.EntityFramework.Core;

[AppDbContext("MoYuRazorApi", DbProvider.Sqlite)]
public class DefaultDbContext : AppDbContext<DefaultDbContext>
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
    {
    }
}
