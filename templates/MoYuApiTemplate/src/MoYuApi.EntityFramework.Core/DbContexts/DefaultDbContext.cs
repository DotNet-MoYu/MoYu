using MoYu.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;

namespace MoYuApi.EntityFramework.Core;

[AppDbContext("MoYuApi", DbProvider.Sqlite)]
public class DefaultDbContext : AppDbContext<DefaultDbContext>
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
    {
    }
}
