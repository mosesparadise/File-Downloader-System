using Microsoft.EntityFrameworkCore.Migrations;

namespace FileDownloaderSystem.Migrations
{
    public partial class AddedProtocolField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Protocol",
                table: "DownloadLog",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Protocol",
                table: "DownloadLog");
        }
    }
}
