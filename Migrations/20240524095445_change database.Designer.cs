﻿// <auto-generated />
using System;
using AngularApi.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AngularApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240524095445_change database")]
    partial class changedatabase
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("AngularApi.Models.Booking", b =>
                {
                    b.Property<int>("BookingID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("BookingID"), 1L, 1);

                    b.Property<DateTime>("Booking_Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Booking_Type")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("End_Date")
                        .HasColumnType("datetime2");

                    b.Property<int?>("ID")
                        .HasColumnType("int");

                    b.Property<DateTime>("Start_Date")
                        .HasColumnType("datetime2");

                    b.HasKey("BookingID");

                    b.HasIndex("ID");

                    b.ToTable("bookings", (string)null);
                });

            modelBuilder.Entity("AngularApi.Models.Coupon", b =>
                {
                    b.Property<int>("CouponId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CouponId"), 1L, 1);

                    b.Property<int?>("BookingId")
                        .HasColumnType("int");

                    b.Property<string>("CouponCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CouponExpiry")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("End_date")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Start_date")
                        .HasColumnType("datetime2");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("CouponId");

                    b.HasIndex("BookingId");

                    b.HasIndex("UserId");

                    b.ToTable("coupons", (string)null);
                });

            modelBuilder.Entity("AngularApi.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Confirmpsw")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ResetPasswordExpiry")
                        .HasColumnType("datetime2");

                    b.Property<string>("ResetPasswordToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Token")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("AngularApi.Models.Booking", b =>
                {
                    b.HasOne("AngularApi.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("ID");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AngularApi.Models.Coupon", b =>
                {
                    b.HasOne("AngularApi.Models.Booking", "Booking")
                        .WithMany()
                        .HasForeignKey("BookingId");

                    b.HasOne("AngularApi.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Booking");

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
