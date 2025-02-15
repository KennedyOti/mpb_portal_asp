using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MPB_PORTAL_WEB_APP.Migrations
{
    /// <inheritdoc />
    public partial class PagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AssignedStaffId",
                table: "Cases",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MetaTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MetaDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cases_AssignedStaffId",
                table: "Cases",
                column: "AssignedStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_AspNetUsers_AssignedStaffId",
                table: "Cases",
                column: "AssignedStaffId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_AspNetUsers_AssignedStaffId",
                table: "Cases");

            migrationBuilder.DropTable(
                name: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Cases_AssignedStaffId",
                table: "Cases");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedStaffId",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
