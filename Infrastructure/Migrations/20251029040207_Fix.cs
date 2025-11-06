using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketAssignees_Tickets_TicketId1",
                table: "TicketAssignees");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketAssignees_Users_AssigneeId1",
                table: "TicketAssignees");

            migrationBuilder.DropIndex(
                name: "IX_TicketAssignees_AssigneeId1",
                table: "TicketAssignees");

            migrationBuilder.DropIndex(
                name: "IX_TicketAssignees_TicketId1",
                table: "TicketAssignees");

            migrationBuilder.DropColumn(
                name: "AssigneeId1",
                table: "TicketAssignees");

            migrationBuilder.DropColumn(
                name: "TicketId1",
                table: "TicketAssignees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssigneeId1",
                table: "TicketAssignees",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TicketId1",
                table: "TicketAssignees",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TicketAssignees_AssigneeId1",
                table: "TicketAssignees",
                column: "AssigneeId1");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAssignees_TicketId1",
                table: "TicketAssignees",
                column: "TicketId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAssignees_Tickets_TicketId1",
                table: "TicketAssignees",
                column: "TicketId1",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAssignees_Users_AssigneeId1",
                table: "TicketAssignees",
                column: "AssigneeId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
