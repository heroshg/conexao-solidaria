using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campaigns.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignConcurrencyToken : Migration
    {
        // No-op on purpose: "xmin" is a Postgres SYSTEM column already present on every table —
        // it can't be added via AddColumn (it isn't a real column to create). This migration
        // exists only so the model snapshot matches CampaignConfiguration's new xmin shadow
        // property; scaffolding generated a literal AddColumn here, which would fail against a
        // column Postgres already provides, so both directions are intentionally empty.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
