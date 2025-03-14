using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace minitwit.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PubDateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_messages_pubdate",
                table: "messages",
                column: "pubdate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_messages_pubdate",
                table: "messages");
        }
    }
}
