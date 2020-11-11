using Microsoft.EntityFrameworkCore.Migrations;

namespace Firebend.AutoCrud.Web.Sample.Migrations
{
    public partial class AddNickname : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NickName",
                table: "EfPeople",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NickName",
                table: "EfPeople");
        }
    }
}
