using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiapCloudGames.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class create_games : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Games",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Developer",
                table: "Games",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Games",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleaseDate",
                table: "Games",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "UQ_Game_Title_Developer",
                table: "Games",
                columns: new[] { "Title", "Developer" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_Game_Title_Developer",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Developer",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "Games");
        }
    }
}
