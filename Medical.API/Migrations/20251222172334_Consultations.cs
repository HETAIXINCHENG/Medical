using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medical.API.Migrations
{
    /// <inheritdoc />
    public partial class Consultations : Migration
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

            // 检查并删除索引（如果存在）
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF EXISTS (
                        SELECT 1 
                        FROM pg_indexes 
                        WHERE indexname = 'IX_Consultations_PatientId'
                    ) THEN
                        DROP INDEX IF EXISTS ""IX_Consultations_PatientId"";
                    END IF;
                END $$;
            ");

            // 检查并删除列（如果存在）
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'Consultations' AND column_name = 'PatientId'
                    ) THEN
                        ALTER TABLE ""Consultations"" DROP COLUMN ""PatientId"";
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "Consultations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_PatientId",
                table: "Consultations",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Patients_PatientId",
                table: "Consultations",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");
        }
    }
}
