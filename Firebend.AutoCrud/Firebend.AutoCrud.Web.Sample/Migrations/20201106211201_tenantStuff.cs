using Microsoft.EntityFrameworkCore.Migrations;

namespace Firebend.AutoCrud.Web.Sample.Migrations
{
    public partial class tenantStuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "EfPeople",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EfPeople");
        }
    }
}
