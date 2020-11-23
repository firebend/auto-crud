using Microsoft.EntityFrameworkCore.Migrations;

namespace Firebend.AutoCrud.Web.Sample.Migrations
{
    public partial class AddFullTextIndexCatalog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(
            sql: "CREATE FULLTEXT CATALOG ftCatalog AS DEFAULT;",
            suppressTransaction: true);

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
