using Microsoft.EntityFrameworkCore.Migrations;

namespace Firebend.AutoCrud.Web.Sample.Migrations
{
#pragma warning disable IDE1006
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
#pragma warning restore IDE1006
}
