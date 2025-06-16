using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionFinanciere.API.Migrations
{
    /// <inheritdoc />
    public partial class AjoutNotificationsEtChamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SeuilAlerte",
                table: "Comptes",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BudgetMensuel",
                table: "Categories",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Icone",
                table: "Categories",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Titre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EstLue = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DonneesContexte = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BudgetMensuel", "CreatedAt", "Icone" },
                values: new object[] { 0m, new DateTime(2025, 6, 16, 6, 33, 2, 958, DateTimeKind.Utc).AddTicks(2165), "shopping-cart" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BudgetMensuel", "CreatedAt", "Icone" },
                values: new object[] { 0m, new DateTime(2025, 6, 16, 6, 33, 2, 958, DateTimeKind.Utc).AddTicks(2172), "car" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BudgetMensuel", "CreatedAt", "Icone" },
                values: new object[] { 0m, new DateTime(2025, 6, 16, 6, 33, 2, 958, DateTimeKind.Utc).AddTicks(2174), "home" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BudgetMensuel", "CreatedAt", "Icone" },
                values: new object[] { 0m, new DateTime(2025, 6, 16, 6, 33, 2, 958, DateTimeKind.Utc).AddTicks(2175), "film" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BudgetMensuel", "CreatedAt", "Icone" },
                values: new object[] { 0m, new DateTime(2025, 6, 16, 6, 33, 2, 958, DateTimeKind.Utc).AddTicks(2177), "heart" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "BudgetMensuel", "CreatedAt", "Icone" },
                values: new object[] { 0m, new DateTime(2025, 6, 16, 6, 33, 2, 958, DateTimeKind.Utc).AddTicks(2178), "wallet" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "BudgetMensuel", "CreatedAt", "Icone" },
                values: new object[] { 0m, new DateTime(2025, 6, 16, 6, 33, 2, 958, DateTimeKind.Utc).AddTicks(2180), "laptop" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "BudgetMensuel", "CreatedAt", "Icone" },
                values: new object[] { 0m, new DateTime(2025, 6, 16, 6, 33, 2, 958, DateTimeKind.Utc).AddTicks(2182), "chart-line" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropColumn(
                name: "SeuilAlerte",
                table: "Comptes");

            migrationBuilder.DropColumn(
                name: "BudgetMensuel",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Icone",
                table: "Categories");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 12, 9, 38, 803, DateTimeKind.Utc).AddTicks(4356));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 12, 9, 38, 803, DateTimeKind.Utc).AddTicks(4360));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 12, 9, 38, 803, DateTimeKind.Utc).AddTicks(4361));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 12, 9, 38, 803, DateTimeKind.Utc).AddTicks(4362));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 12, 9, 38, 803, DateTimeKind.Utc).AddTicks(4363));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 12, 9, 38, 803, DateTimeKind.Utc).AddTicks(4364));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 12, 9, 38, 803, DateTimeKind.Utc).AddTicks(4365));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 12, 9, 38, 803, DateTimeKind.Utc).AddTicks(4366));
        }
    }
}
