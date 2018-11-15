using Microsoft.EntityFrameworkCore.Migrations;

namespace FileDownloaderSystem.Migrations
{
    public partial class AddedErrorMessageField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "DownloadLog",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "DownloadLog");
        }
    }
}
