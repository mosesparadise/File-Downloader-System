﻿// <auto-generated />
using System;
using FileDownloaderSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FileDownloaderSystem.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20181111202601_AddedProtocolField")]
    partial class AddedProtocolField
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("FileDownloaderSystem.Data.Entities.DownloadLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("BenchmarkSize");

                    b.Property<int>("BenchmarkSpeed");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<double>("DownloadSpeed");

                    b.Property<int>("DownloadStatus");

                    b.Property<DateTime>("EndDownload");

                    b.Property<string>("ErrorMessage");

                    b.Property<string>("FileDestination");

                    b.Property<string>("FileSource");

                    b.Property<float>("PercentageOfFailure");

                    b.Property<string>("Protocol");

                    b.Property<DateTime>("StartDownload");

                    b.HasKey("Id");

                    b.ToTable("DownloadLog");
                });
#pragma warning restore 612, 618
        }
    }
}
