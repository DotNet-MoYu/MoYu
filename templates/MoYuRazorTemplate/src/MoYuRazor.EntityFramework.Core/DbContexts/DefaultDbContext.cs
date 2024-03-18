using MoYu.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;

namespace MoYuRazor.EntityFramework.Core;

[AppDbContext("MoYuRazor", DbProvider.Sqlite)]
public class DefaultDbContext : AppDbContext<DefaultDbContext>
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
    {
    }
}
