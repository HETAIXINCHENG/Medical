using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medical.API.Migrations
{
    /// <inheritdoc />
    public partial class SenderId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "ConsultationMessages",
                newName: "SenderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "ConsultationMessages",
                newName: "PatientId");
        }
    }
}
