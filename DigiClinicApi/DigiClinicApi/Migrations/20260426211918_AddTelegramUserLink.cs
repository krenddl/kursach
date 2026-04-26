using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigiClinicApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramUserLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TelegramChatId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramLinkCode",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TelegramLinkCodeExpiresAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TelegramChatId",
                table: "Users",
                column: "TelegramChatId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_TelegramChatId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TelegramChatId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TelegramLinkCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TelegramLinkCodeExpiresAt",
                table: "Users");
        }
    }
}
