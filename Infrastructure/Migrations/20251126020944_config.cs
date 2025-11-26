using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class config : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_ImplementationPlans_ImplementationPlanId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_HeadDepartmentId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "ImplementationPlans");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_HeadDepartmentId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ImplementationPlanId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "HeadDepartmentId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ImplementationPlanId",
                table: "Tickets");

            migrationBuilder.AddColumn<string>(
                name: "ImplementationPlan",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TicketHeads",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    HeadId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHeads", x => new { x.TicketId, x.HeadId });
                    table.ForeignKey(
                        name: "FK_TicketHeads_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketHeads_Users_HeadId",
                        column: x => x.HeadId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketHeads_HeadId",
                table: "TicketHeads",
                column: "HeadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketHeads");

            migrationBuilder.DropColumn(
                name: "ImplementationPlan",
                table: "Tickets");

            migrationBuilder.AddColumn<int>(
                name: "HeadDepartmentId",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ImplementationPlanId",
                table: "Tickets",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImplementationPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImplementationPlans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_HeadDepartmentId",
                table: "Tickets",
                column: "HeadDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ImplementationPlanId",
                table: "Tickets",
                column: "ImplementationPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_ImplementationPlans_ImplementationPlanId",
                table: "Tickets",
                column: "ImplementationPlanId",
                principalTable: "ImplementationPlans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_HeadDepartmentId",
                table: "Tickets",
                column: "HeadDepartmentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
