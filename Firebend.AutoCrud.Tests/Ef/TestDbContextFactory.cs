using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.Tests.Ef;

public static class TestDbContextFactory
{
    public static TestContext Create()
    {
        var opt = new DbContextOptionsBuilder<TestContext>()
            .UseSqlServer()
            .Options;

        return new TestContext(opt);
    }
}
