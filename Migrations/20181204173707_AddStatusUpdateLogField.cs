using Microsoft.EntityFrameworkCore.Migrations;

namespace MttApi.Migrations
{
    public partial class AddStatusUpdateLogField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatusUpdateLog",
                table: "Records",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusUpdateLog",
                table: "Records");
        }
    }
}
