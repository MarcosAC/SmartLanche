using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLanche.Migrations
{
    /// <inheritdoc />
    public partial class RenameClientObservationColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Observation",
                table: "Clients",
                newName: "Observations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Observations",
                table: "Clients",
                newName: "Observation");
        }
    }
}
