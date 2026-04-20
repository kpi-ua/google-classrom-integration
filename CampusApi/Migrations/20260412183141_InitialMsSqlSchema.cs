using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialMsSqlSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cmonitoring",
                columns: table => new
                {
                    CMonitoringId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CRNPRowId = table.Column<int>(type: "int", nullable: false),
                    RtStudyGroupId = table.Column<int>(type: "int", nullable: false),
                    VcActuality = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    VcChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SemesterYear = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cmonitoring", x => x.CMonitoringId);
                });

            migrationBuilder.CreateTable(
                name: "cmonitoringcolumn",
                columns: table => new
                {
                    CMonitoringColumnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CMonitoringEmployeeId = table.Column<int>(type: "int", nullable: false),
                    DcLoadId = table.Column<int>(type: "int", nullable: false),
                    DateMark = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cmonitoringcolumn", x => x.CMonitoringColumnId);
                });

            migrationBuilder.CreateTable(
                name: "cmonitoringemployee",
                columns: table => new
                {
                    CMonitoringEmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CMonitoringId = table.Column<int>(type: "int", nullable: false),
                    EEmployees1Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VcActuality = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    VcChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cmonitoringemployee", x => x.CMonitoringEmployeeId);
                });

            migrationBuilder.CreateTable(
                name: "cmonitoringrow",
                columns: table => new
                {
                    CMonitoringRowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CMonitoringColumnId = table.Column<int>(type: "int", nullable: false),
                    SPersonalityId = table.Column<int>(type: "int", nullable: false),
                    CMonitoringStudentId = table.Column<int>(type: "int", nullable: false),
                    Mark = table.Column<float>(type: "real", nullable: true),
                    VcActuality = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    VcChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cmonitoringrow", x => x.CMonitoringRowId);
                });

            migrationBuilder.CreateTable(
                name: "cmonitoringstudent",
                columns: table => new
                {
                    CMonitoringStudentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CMonitoringEmployeeId = table.Column<int>(type: "int", nullable: false),
                    SPersonalityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cmonitoringstudent", x => x.CMonitoringStudentId);
                });

            migrationBuilder.CreateTable(
                name: "eemployees1",
                columns: table => new
                {
                    EEmployees1Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserAccountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eemployees1", x => x.EEmployees1Id);
                });

            migrationBuilder.CreateTable(
                name: "spersonality1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    STUDENT_ID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FIRST_NAME = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LAST_NAME = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spersonality1", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "useraccount",
                columns: table => new
                {
                    UserAccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Gmail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_useraccount", x => x.UserAccountId);
                });

            migrationBuilder.CreateTable(
                name: "usercontact",
                columns: table => new
                {
                    UserContactId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserAccountId = table.Column<int>(type: "int", nullable: false),
                    DcContactTypeId = table.Column<int>(type: "int", nullable: false),
                    UserContactValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SPersonalityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usercontact", x => x.UserContactId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cmonitoring");

            migrationBuilder.DropTable(
                name: "cmonitoringcolumn");

            migrationBuilder.DropTable(
                name: "cmonitoringemployee");

            migrationBuilder.DropTable(
                name: "cmonitoringrow");

            migrationBuilder.DropTable(
                name: "cmonitoringstudent");

            migrationBuilder.DropTable(
                name: "eemployees1");

            migrationBuilder.DropTable(
                name: "spersonality1");

            migrationBuilder.DropTable(
                name: "useraccount");

            migrationBuilder.DropTable(
                name: "usercontact");
        }
    }
}
