using Microsoft.EntityFrameworkCore.Migrations;

namespace Firebend.AutoCrud.Web.Sample.Migrations;

public partial class Testing_Other_Email : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "NotEmail",
            table: "EfPeople",
            type: "nvarchar(300)",
            maxLength: 300,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_EfPeople_NotEmail",
            table: "EfPeople",
            column: "NotEmail",
            unique: true,
            filter: "[NotEmail] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_EfPeople_NotEmail",
            table: "EfPeople");

        migrationBuilder.DropColumn(
            name: "NotEmail",
            table: "EfPeople");
    }
}
