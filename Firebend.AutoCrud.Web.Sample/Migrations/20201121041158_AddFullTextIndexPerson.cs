using Microsoft.EntityFrameworkCore.Migrations;

namespace Firebend.AutoCrud.Web.Sample.Migrations
{
    public partial class AddFullTextIndexPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(
            sql: "CREATE FULLTEXT INDEX ON EfPeople(FirstName,LastName,NickName) KEY INDEX PK_EfPeople",
            suppressTransaction: true);

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
