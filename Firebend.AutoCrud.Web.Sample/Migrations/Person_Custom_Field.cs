using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Firebend.AutoCrud.Web.Sample.Migrations;

public partial class Person_Custom_Field : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "EfPeople_CustomFields",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<int>(type: "int", nullable: false),
                EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EfPeople_CustomFields", x => x.Id)
                    .Annotation("SqlServer:Clustered", false);
                table.ForeignKey(
                    name: "FK_EfPeople_CustomFields_EfPeople_EntityId",
                    column: x => x.EntityId,
                    principalTable: "EfPeople",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Pets_CustomFields",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<int>(type: "int", nullable: false),
                EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Pets_CustomFields", x => x.Id)
                    .Annotation("SqlServer:Clustered", false);
                table.ForeignKey(
                    name: "FK_Pets_CustomFields_Pets_EntityId",
                    column: x => x.EntityId,
                    principalTable: "Pets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_EfPeople_CustomFields_EntityId",
            table: "EfPeople_CustomFields",
            column: "EntityId")
            .Annotation("SqlServer:Clustered", true);

        migrationBuilder.CreateIndex(
            name: "IX_Pets_CustomFields_EntityId",
            table: "Pets_CustomFields",
            column: "EntityId")
            .Annotation("SqlServer:Clustered", true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "EfPeople_CustomFields");

        migrationBuilder.DropTable(
            name: "Pets_CustomFields");
    }
}
