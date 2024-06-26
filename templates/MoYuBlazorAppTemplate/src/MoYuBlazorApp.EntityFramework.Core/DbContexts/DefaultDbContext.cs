using MoYu.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;

namespace MoYuBlazorApp.EntityFramework.Core;

[AppDbContext("MoYuBlazorApp", DbProvider.Sqlite)]
public class DefaultDbContext : AppDbContext<DefaultDbContext>
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
    {
    }
}
