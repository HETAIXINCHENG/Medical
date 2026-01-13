using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medical.API.Migrations
{
    /// <inheritdoc />
    public partial class FollowerCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FollowerCount",
                table: "Doctors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalReadCount",
                table: "Doctors",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowerCount",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "TotalReadCount",
                table: "Doctors");
        }
    }
}
