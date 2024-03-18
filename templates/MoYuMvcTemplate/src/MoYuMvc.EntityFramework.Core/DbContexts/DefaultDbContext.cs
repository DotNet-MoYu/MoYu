using MoYu.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;

namespace MoYuMvc.EntityFramework.Core;

[AppDbContext("MoYuMvc", DbProvider.Sqlite)]
public class DefaultDbContext : AppDbContext<DefaultDbContext>
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
    {
    }
}
