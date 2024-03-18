using MoYu.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;

namespace MoYuApp.EntityFramework.Core;

[AppDbContext("MoYuApp", DbProvider.Sqlite)]
public class DefaultDbContext : AppDbContext<DefaultDbContext>
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
    {
    }
}
