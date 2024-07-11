using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Abstractions;

public class AbstractDbContext: DbContext, IDbContext
{
    public AbstractDbContext(DbContextOptions options) : base(options)
    {
        Options = options;
    }

    public DbContextOptions Options { get; }

    public bool UseUserDefinedTransaction { get; set; }
}
