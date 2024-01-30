using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Firebend.AutoCrud.Web.Sample.Migrations;

public partial class AddDataAuth : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DataAuth",
            table: "Pets",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DataAuth",
            table: "EfPeople",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DataAuth",
            table: "Pets");

        migrationBuilder.DropColumn(
            name: "DataAuth",
            table: "EfPeople");

    }
}
