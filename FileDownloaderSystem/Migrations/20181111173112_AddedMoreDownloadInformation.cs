using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileDownloaderSystem.Migrations
{
    public partial class AddedMoreDownloadInformation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "DownloadLog",
                nullable: false,
                defaultValueSql: "getdate()");

            migrationBuilder.AddColumn<double>(
                name: "DownloadSpeed",
                table: "DownloadLog",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "DownloadLog");

            migrationBuilder.DropColumn(
                name: "DownloadSpeed",
                table: "DownloadLog");
        }
    }
}
