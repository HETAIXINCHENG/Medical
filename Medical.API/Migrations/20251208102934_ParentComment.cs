using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medical.API.Migrations
{
    /// <inheritdoc />
    public partial class ParentComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_Patients_PatientId",
                table: "PostComments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostLikes_Patients_PatientId",
                table: "PostLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Patients_PatientId",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "Posts",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_PatientId",
                table: "Posts",
                newName: "IX_Posts_UserId");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "PostLikes",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PostLikes_PostId_PatientId",
                table: "PostLikes",
                newName: "IX_PostLikes_PostId_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PostLikes_PatientId",
                table: "PostLikes",
                newName: "IX_PostLikes_UserId");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "PostComments",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PostComments_PatientId",
                table: "PostComments",
                newName: "IX_PostComments_UserId");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrls",
                table: "Posts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrls",
                table: "PostComments",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_Users_UserId",
                table: "PostComments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PostLikes_Users_UserId",
                table: "PostLikes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Users_UserId",
                table: "Posts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_Users_UserId",
                table: "PostComments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostLikes_Users_UserId",
                table: "PostLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Users_UserId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "AttachmentUrls",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "AttachmentUrls",
                table: "PostComments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Posts",
                newName: "PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_UserId",
                table: "Posts",
                newName: "IX_Posts_PatientId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PostLikes",
                newName: "PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_PostLikes_UserId",
                table: "PostLikes",
                newName: "IX_PostLikes_PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_PostLikes_PostId_UserId",
                table: "PostLikes",
                newName: "IX_PostLikes_PostId_PatientId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PostComments",
                newName: "PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_PostComments_UserId",
                table: "PostComments",
                newName: "IX_PostComments_PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_Patients_PatientId",
                table: "PostComments",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PostLikes_Patients_PatientId",
                table: "PostLikes",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Patients_PatientId",
                table: "Posts",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
