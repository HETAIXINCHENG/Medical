using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medical.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTertiaryHospitals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TertiaryHospitals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProvinceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TertiaryHospitals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TertiaryHospitals_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TertiaryHospitals_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TertiaryHospitals_Name",
                table: "TertiaryHospitals",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TertiaryHospitals_ProvinceId",
                table: "TertiaryHospitals",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_TertiaryHospitals_CityId",
                table: "TertiaryHospitals",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_TertiaryHospitals_ProvinceId_CityId",
                table: "TertiaryHospitals",
                columns: new[] { "ProvinceId", "CityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TertiaryHospitals");
        }
    }
}

