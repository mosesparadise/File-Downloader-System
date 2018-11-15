using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileDownloaderSystem.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FileSource = table.Column<string>(nullable: true),
                    FileDestination = table.Column<string>(nullable: true),
                    StartDownload = table.Column<DateTime>(nullable: false),
                    EndDownload = table.Column<DateTime>(nullable: false),
                    BenchmarkSize = table.Column<int>(nullable: false),
                    BenchmarkSpeed = table.Column<int>(nullable: false),
                    PercentageOfFailure = table.Column<float>(nullable: false),
                    DownloadStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadLog", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadLog");
        }
    }
}
