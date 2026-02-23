using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundialitoCorporativo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResponseContentTypeToIdempotencyRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResponseContentType",
                table: "IdempotencyRecords",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponseContentType",
                table: "IdempotencyRecords");
        }
    }
}
