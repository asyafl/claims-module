using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClaimsModule.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Claims",
                columns: new[] { "Id", "AssignedHandlerId", "ClaimNumber", "ClientName", "ClosedAt", "ClosureReason", "CreatedAt", "DeletedAt", "IsDeleted", "ManagerOverrideFlag", "Notes", "OrganisationId", "PolicyId", "PolicyNumber", "ReportedDate", "Severity", "Status", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[,]
                {
                    { new Guid("33000000-0000-0000-0000-000000000001"), null, "CLM-2025-0000001", "Harborview Properties Inc", null, null, new DateTimeOffset(new DateTime(2025, 1, 20, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, false, null, new Guid("00000000-0000-0000-0000-000000000001"), new Guid("22000000-0000-0000-0000-000000000002"), "POL-2024-001002", new DateTimeOffset(new DateTime(2025, 1, 20, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "Draft", null, null, null },
                    { new Guid("33000000-0000-0000-0000-000000000002"), null, "CLM-2025-0000002", "Meridian Transport LLC", null, null, new DateTimeOffset(new DateTime(2025, 2, 10, 11, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, false, null, new Guid("00000000-0000-0000-0000-000000000001"), new Guid("22000000-0000-0000-0000-000000000001"), "POL-2024-001001", new DateTimeOffset(new DateTime(2025, 2, 10, 11, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "Open", null, null, null },
                    { new Guid("33000000-0000-0000-0000-000000000003"), null, "CLM-2025-0000003", "Stanton Medical Group", null, null, new DateTimeOffset(new DateTime(2025, 3, 5, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, false, null, new Guid("00000000-0000-0000-0000-000000000001"), new Guid("22000000-0000-0000-0000-000000000004"), "POL-2025-002002", new DateTimeOffset(new DateTime(2025, 3, 5, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "UnderInvestigation", null, null, null },
                    { new Guid("33000000-0000-0000-0000-000000000004"), null, "CLM-2025-0000004", "Coastal Builders Group", null, null, new DateTimeOffset(new DateTime(2024, 11, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, false, null, new Guid("00000000-0000-0000-0000-000000000001"), new Guid("22000000-0000-0000-0000-000000000003"), "POL-2025-002001", new DateTimeOffset(new DateTime(2024, 11, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "PendingPayment", null, null, null },
                    { new Guid("33000000-0000-0000-0000-000000000005"), null, "CLM-2025-0000005", "Meridian Transport LLC", new DateTimeOffset(new DateTime(2025, 1, 10, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "All parties settled. Reserves reconciled.", new DateTimeOffset(new DateTime(2024, 8, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, false, null, new Guid("00000000-0000-0000-0000-000000000001"), new Guid("22000000-0000-0000-0000-000000000001"), "POL-2024-001001", new DateTimeOffset(new DateTime(2024, 8, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "Closed", null, null, null },
                    { new Guid("33000000-0000-0000-0000-000000000006"), null, "CLM-2025-0000006", "Harborview Properties Inc", null, null, new DateTimeOffset(new DateTime(2025, 4, 3, 16, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, false, null, new Guid("00000000-0000-0000-0000-000000000001"), new Guid("22000000-0000-0000-0000-000000000002"), "POL-2024-001002", new DateTimeOffset(new DateTime(2025, 4, 3, 16, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "Withdrawn", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "ClaimParties",
                columns: new[] { "Id", "ClaimId", "CompanyName", "CreatedAt", "DeletedAt", "Email", "FirstName", "IsActive", "IsDeleted", "LastName", "Notes", "OrganisationId", "PartyRole", "PartyType", "Phone", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[,]
                {
                    { new Guid("55000000-0000-0000-0000-000000000001"), new Guid("33000000-0000-0000-0000-000000000001"), "Harborview Properties Inc", new DateTimeOffset(new DateTime(2025, 1, 20, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "claims@harborview.ua", null, true, false, null, null, new Guid("00000000-0000-0000-0000-000000000001"), "Insured", "Company", "+380442001122", null, null, null },
                    { new Guid("55000000-0000-0000-0000-000000000002"), new Guid("33000000-0000-0000-0000-000000000002"), null, new DateTimeOffset(new DateTime(2025, 2, 10, 11, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "o.kovalenko@meridian.ua", "Oleksandr", true, false, "Kovalenko", null, new Guid("00000000-0000-0000-0000-000000000001"), "Claimant", "Person", "+380501234567", null, null, null },
                    { new Guid("55000000-0000-0000-0000-000000000003"), new Guid("33000000-0000-0000-0000-000000000003"), "Stanton Medical Group", new DateTimeOffset(new DateTime(2025, 3, 5, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "legal@stanton.ua", null, true, false, null, null, new Guid("00000000-0000-0000-0000-000000000001"), "Claimant", "Company", "+380487654321", null, null, null },
                    { new Guid("55000000-0000-0000-0000-000000000004"), new Guid("33000000-0000-0000-0000-000000000004"), "Coastal Builders Group", new DateTimeOffset(new DateTime(2024, 11, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "insurance@coastal.ua", null, true, false, null, null, new Guid("00000000-0000-0000-0000-000000000001"), "Claimant", "Company", "+380661112233", null, null, null },
                    { new Guid("55000000-0000-0000-0000-000000000005"), new Guid("33000000-0000-0000-0000-000000000005"), "Meridian Transport LLC", new DateTimeOffset(new DateTime(2024, 8, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "claims@meridian.ua", null, true, false, null, null, new Guid("00000000-0000-0000-0000-000000000001"), "Claimant", "Company", "+380991234567", null, null, null },
                    { new Guid("55000000-0000-0000-0000-000000000006"), new Guid("33000000-0000-0000-0000-000000000006"), null, new DateTimeOffset(new DateTime(2025, 4, 3, 16, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "v.bondarenko@gmail.com", "Viktor", true, false, "Bondarenko", null, new Guid("00000000-0000-0000-0000-000000000001"), "Claimant", "Person", "+380733332211", null, null, null },
                    { new Guid("55000000-0000-0000-0000-000000000007"), new Guid("33000000-0000-0000-0000-000000000003"), null, new DateTimeOffset(new DateTime(2025, 3, 5, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Natalia", true, false, "Sydorenko", null, new Guid("00000000-0000-0000-0000-000000000001"), "Witness", "Person", "+380509876543", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "LossEvents",
                columns: new[] { "Id", "CauseOfLossCode", "ClaimId", "CreatedAt", "DeletedAt", "EstimatedLossAmount", "IsDeleted", "LossDate", "LossDescription", "LossLocation", "OrganisationId", "PoliceReportNumber", "ReportDate", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[,]
                {
                    { new Guid("44000000-0000-0000-0000-000000000001"), "COL-FIRE", new Guid("33000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2025, 1, 20, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 45000m, false, new DateTimeOffset(new DateTime(2025, 1, 18, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Electrical fire originated in server room causing damage to equipment and partial structural damage to the building.", "Kyiv, Khreshchatyk 1, Building B", new Guid("00000000-0000-0000-0000-000000000001"), null, new DateTimeOffset(new DateTime(2025, 1, 20, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { new Guid("44000000-0000-0000-0000-000000000002"), "COL-VEH-COL", new Guid("33000000-0000-0000-0000-000000000002"), new DateTimeOffset(new DateTime(2025, 2, 10, 11, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 18500m, false, new DateTimeOffset(new DateTime(2025, 2, 8, 10, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Head-on collision at intersection resulted in significant front-end damage to company vehicle and minor injuries to driver.", "Lviv, Svobody Ave 12", new Guid("00000000-0000-0000-0000-000000000001"), null, new DateTimeOffset(new DateTime(2025, 2, 10, 11, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { new Guid("44000000-0000-0000-0000-000000000003"), "COL-THEFT", new Guid("33000000-0000-0000-0000-000000000003"), new DateTimeOffset(new DateTime(2025, 3, 5, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 120000m, false, new DateTimeOffset(new DateTime(2025, 3, 3, 2, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Pharmaceutical storage area broken into overnight. Multiple controlled substance cabinets forced open. Police investigation ongoing.", "Odesa, Derybasivska 5, Medical Center", new Guid("00000000-0000-0000-0000-000000000001"), "OD-2025-004421", new DateTimeOffset(new DateTime(2025, 3, 5, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { new Guid("44000000-0000-0000-0000-000000000004"), "COL-FLOOD", new Guid("33000000-0000-0000-0000-000000000004"), new DateTimeOffset(new DateTime(2024, 11, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 87000m, false, new DateTimeOffset(new DateTime(2024, 11, 12, 6, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Heavy rainfall caused flash flooding in construction site basement. Equipment submerged, foundation work delayed three weeks.", "Dnipro, Soborna 8, Construction Site", new Guid("00000000-0000-0000-0000-000000000001"), null, new DateTimeOffset(new DateTime(2024, 11, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { new Guid("44000000-0000-0000-0000-000000000005"), "COL-WIND", new Guid("33000000-0000-0000-0000-000000000005"), new DateTimeOffset(new DateTime(2024, 8, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 32000m, false, new DateTimeOffset(new DateTime(2024, 7, 28, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Severe wind storm damaged roof of cargo warehouse causing water ingress. Cargo contents partially destroyed.", "Kharkiv, Industrial Zone, Warehouse 14", new Guid("00000000-0000-0000-0000-000000000001"), null, new DateTimeOffset(new DateTime(2024, 8, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null },
                    { new Guid("44000000-0000-0000-0000-000000000006"), "COL-INJURY", new Guid("33000000-0000-0000-0000-000000000006"), new DateTimeOffset(new DateTime(2025, 4, 3, 16, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 15000m, false, new DateTimeOffset(new DateTime(2025, 4, 1, 11, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Third party slip and fall incident in office lobby. Claimant later withdrew citing pre-existing medical condition.", "Kyiv, Business Center, Entrance Hall", new Guid("00000000-0000-0000-0000-000000000001"), null, new DateTimeOffset(new DateTime(2025, 4, 3, 16, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ClaimParties",
                keyColumn: "Id",
                keyValue: new Guid("55000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "ClaimParties",
                keyColumn: "Id",
                keyValue: new Guid("55000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "ClaimParties",
                keyColumn: "Id",
                keyValue: new Guid("55000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "ClaimParties",
                keyColumn: "Id",
                keyValue: new Guid("55000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "ClaimParties",
                keyColumn: "Id",
                keyValue: new Guid("55000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "ClaimParties",
                keyColumn: "Id",
                keyValue: new Guid("55000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "ClaimParties",
                keyColumn: "Id",
                keyValue: new Guid("55000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "LossEvents",
                keyColumn: "Id",
                keyValue: new Guid("44000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "LossEvents",
                keyColumn: "Id",
                keyValue: new Guid("44000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "LossEvents",
                keyColumn: "Id",
                keyValue: new Guid("44000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "LossEvents",
                keyColumn: "Id",
                keyValue: new Guid("44000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "LossEvents",
                keyColumn: "Id",
                keyValue: new Guid("44000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "LossEvents",
                keyColumn: "Id",
                keyValue: new Guid("44000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: new Guid("33000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: new Guid("33000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: new Guid("33000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: new Guid("33000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: new Guid("33000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: new Guid("33000000-0000-0000-0000-000000000006"));
        }
    }
}
