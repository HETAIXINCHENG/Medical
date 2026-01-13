using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medical.API.Migrations
{
    /// <inheritdoc />
    public partial class DoctorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 检查并删除外键约束（如果存在）
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF EXISTS (
                        SELECT 1 
                        FROM pg_constraint 
                        WHERE conname = 'FK_Consultations_Patients_PatientId'
                    ) THEN
                        ALTER TABLE ""Consultations"" DROP CONSTRAINT ""FK_Consultations_Patients_PatientId"";
                    END IF;
                END $$;
            ");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "ConsultationMessages",
                newName: "PatientId");

            migrationBuilder.AlterColumn<Guid>(
                name: "PatientId",
                table: "Consultations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "DoctorId",
                table: "ConsultationMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ConsultationPatients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationPatients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationPatients_Consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalTable: "Consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsultationPatients_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPatients_ConsultationId_PatientId",
                table: "ConsultationPatients",
                columns: new[] { "ConsultationId", "PatientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPatients_PatientId",
                table: "ConsultationPatients",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Patients_PatientId",
                table: "Consultations",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 检查并删除外键约束（如果存在）
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF EXISTS (
                        SELECT 1 
                        FROM pg_constraint 
                        WHERE conname = 'FK_Consultations_Patients_PatientId'
                    ) THEN
                        ALTER TABLE ""Consultations"" DROP CONSTRAINT ""FK_Consultations_Patients_PatientId"";
                    END IF;
                END $$;
            ");

            migrationBuilder.DropTable(
                name: "ConsultationPatients");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "ConsultationMessages");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "ConsultationMessages",
                newName: "SenderId");

            migrationBuilder.AlterColumn<Guid>(
                name: "PatientId",
                table: "Consultations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Patients_PatientId",
                table: "Consultations",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
