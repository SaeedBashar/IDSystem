using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentID.Migrations
{
    /// <inheritdoc />
    public partial class addedisactivepropertytoIDcardrelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "IDCards",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "IDCards");
        }
    }
}
