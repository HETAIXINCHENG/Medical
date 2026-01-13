using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medical.API.Migrations
{
    /// <inheritdoc />
    public partial class Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consultations_DoctorId_Status",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_Status",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Consultations");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ConsultationMessages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_DoctorId",
                table: "Consultations",
                column: "DoctorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consultations_DoctorId",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ConsultationMessages");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Consultations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_DoctorId_Status",
                table: "Consultations",
                columns: new[] { "DoctorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_Status",
                table: "Consultations",
                column: "Status");
        }
    }
}
