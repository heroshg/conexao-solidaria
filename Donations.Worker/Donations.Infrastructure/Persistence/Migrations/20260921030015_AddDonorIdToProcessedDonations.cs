using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Donations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDonorIdToProcessedDonations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DonorId",
                schema: "donations",
                table: "ProcessedDonations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DonorId",
                schema: "donations",
                table: "ProcessedDonations");
        }
    }
}
