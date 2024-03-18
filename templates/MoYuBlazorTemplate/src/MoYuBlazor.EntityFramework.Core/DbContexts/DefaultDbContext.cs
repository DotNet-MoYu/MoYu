using MoYu.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;

namespace MoYuBlazor.EntityFramework.Core;

[AppDbContext("MoYuBlazor", DbProvider.Sqlite)]
public class DefaultDbContext : AppDbContext<DefaultDbContext>
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
    {
    }
}
