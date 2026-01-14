using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLanche.Migrations
{
    /// <inheritdoc />
    public partial class RenameOutstandingBalanceColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OutstangingBalance",
                table: "Clients",
                newName: "OutstandingBalance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OutstandingBalance",
                table: "Clients",
                newName: "OutstangingBalance");
        }
    }
}
