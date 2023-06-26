using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentID.Migrations
{
    /// <inheritdoc />
    public partial class AddLectureJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LectureJoins",
                columns: table => new
                {
                    StudentNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LectureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IndexNo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LectureJoins", x => x.StudentNo);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LectureJoins");
        }
    }
}
