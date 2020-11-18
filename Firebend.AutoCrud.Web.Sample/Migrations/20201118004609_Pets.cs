using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Firebend.AutoCrud.Web.Sample.Migrations
{
    public partial class Pets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EfPersonId = table.Column<Guid>(nullable: false),
                    PetName = table.Column<string>(maxLength: 205, nullable: false),
                    PetType = table.Column<string>(maxLength: 250, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(nullable: false),
                    TenantId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pets_EfPeople_EfPersonId",
                        column: x => x.EfPersonId,
                        principalTable: "EfPeople",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pets_EfPersonId",
                table: "Pets",
                column: "EfPersonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pets");
        }
    }
}
