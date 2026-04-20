using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusApi.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleClassroomMappingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleStudentId",
                table: "cmonitoringstudent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleStudentSubmissionId",
                table: "cmonitoringrow",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleCourseWorkId",
                table: "cmonitoringcolumn",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleCourseId",
                table: "cmonitoring",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleStudentId",
                table: "cmonitoringstudent");

            migrationBuilder.DropColumn(
                name: "GoogleStudentSubmissionId",
                table: "cmonitoringrow");

            migrationBuilder.DropColumn(
                name: "GoogleCourseWorkId",
                table: "cmonitoringcolumn");

            migrationBuilder.DropColumn(
                name: "GoogleCourseId",
                table: "cmonitoring");
        }
    }
}
