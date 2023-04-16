using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MacroBot.Migrations
{
    /// <inheritdoc />
    public partial class Initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    user = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    message = table.Column<ulong>(type: "INTEGER", nullable: true),
                    reason = table.Column<string>(type: "TEXT", nullable: false),
                    byuser = table.Column<ulong>(type: "INTEGER", nullable: false),
                    reportid = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.user);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reports");
        }
    }
}
