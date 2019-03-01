﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MttApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MttApi.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20181204173707_AddStatusUpdateLogField")]
    partial class AddStatusUpdateLogField
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("MttApi.Models.BlacklistedWorker", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("WorkerId");

                    b.HasKey("Id");

                    b.ToTable("BlacklistedWorkers");
                });

            modelBuilder.Entity("MttApi.Models.Condition", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ConditionId");

                    b.HasKey("Id");

                    b.ToTable("Conditions");
                });

            modelBuilder.Entity("MttApi.Models.Record", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AssignmentId");

                    b.Property<float>("Bonus");

                    b.Property<bool>("BonusPaid");

                    b.Property<string>("BonusPaymentLog");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("DataString");

                    b.Property<string>("HitId");

                    b.Property<bool>("IsMturk");

                    b.Property<bool>("IsSandbox");

                    b.Property<string>("ReviewLog");

                    b.Property<int>("Status");

                    b.Property<string>("StatusUpdateLog");

                    b.Property<DateTime>("UpdatedAt");

                    b.Property<string>("WorkerId");

                    b.HasKey("Id");

                    b.ToTable("Records");
                });
#pragma warning restore 612, 618
        }
    }
}
