using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechShare.Migrations
{
    /// <inheritdoc />
    public partial class AddLateReturnHandling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualReturnDate",
                table: "Rentals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LateFee",
                table: "Rentals",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualReturnDate",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "LateFee",
                table: "Rentals");
        }
    }
}
