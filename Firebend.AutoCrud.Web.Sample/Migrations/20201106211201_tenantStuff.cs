using Microsoft.EntityFrameworkCore.Migrations;

namespace Firebend.AutoCrud.Web.Sample.Migrations
{
    // ReSharper disable once UnusedType.Global
    // ReSharper disable once InconsistentNaming
    public partial class tenantStuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<int>(
            "TenantId",
            "EfPeople",
            nullable: false,
            defaultValue: 0);

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
            "TenantId",
            "EfPeople");
    }
}
