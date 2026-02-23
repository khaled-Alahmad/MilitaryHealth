using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferredDoctor",
                table: "Consultations");

            migrationBuilder.AddColumn<string>(
                name: "InvestigationReason",
                table: "Investigations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExportedAt",
                table: "FinalDecision",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExportedToRecruitment",
                table: "FinalDecision",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceptionAddedAt",
                table: "FinalDecision",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupervisorAddedAt",
                table: "FinalDecision",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupervisorLastModifiedAt",
                table: "FinalDecision",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorstRefractionLeft",
                table: "EyeExam",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorstRefractionRight",
                table: "EyeExam",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralReason",
                table: "Consultations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QueueNumber",
                table: "Applicants",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "ContractTypes",
                columns: new[] { "ContractTypeID", "Description" },
                values: new object[,]
                {
                    { 1, "مدني" },
                    { 2, "عسكري" },
                    { 3, "متعاقد" }
                });

            migrationBuilder.InsertData(
                table: "MaritalStatus",
                columns: new[] { "MaritalStatusID", "Description" },
                values: new object[,]
                {
                    { 1, "أعزب" },
                    { 2, "متزوج" },
                    { 3, "مطلق" }
                });

            migrationBuilder.InsertData(
                table: "RefractionTypes",
                columns: new[] { "RefractionTypeID", "Description" },
                values: new object[,]
                {
                    { 1, "حسر نظر" },
                    { 2, "مد نظر" },
                    { 3, "استجماتيزم" },
                    { 4, "سليم" }
                });

            migrationBuilder.InsertData(
                table: "Results",
                columns: new[] { "ResultID", "Description" },
                values: new object[,]
                {
                    { 1, "مقبول" },
                    { 2, "مرفوض" },
                    { 3, "مؤجل" }
                });

            migrationBuilder.InsertData(
                table: "Specializations",
                columns: new[] { "SpecializationID", "Description" },
                values: new object[,]
                {
                    { 1, "عيون" },
                    { 2, "باطنة" },
                    { 3, "جراحة" },
                    { 4, "عظمية" },
                    { 5, "اذنية" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContractTypes",
                keyColumn: "ContractTypeID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ContractTypes",
                keyColumn: "ContractTypeID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ContractTypes",
                keyColumn: "ContractTypeID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MaritalStatus",
                keyColumn: "MaritalStatusID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MaritalStatus",
                keyColumn: "MaritalStatusID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MaritalStatus",
                keyColumn: "MaritalStatusID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RefractionTypes",
                keyColumn: "RefractionTypeID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RefractionTypes",
                keyColumn: "RefractionTypeID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RefractionTypes",
                keyColumn: "RefractionTypeID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RefractionTypes",
                keyColumn: "RefractionTypeID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Results",
                keyColumn: "ResultID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Results",
                keyColumn: "ResultID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Results",
                keyColumn: "ResultID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationID",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "InvestigationReason",
                table: "Investigations");

            migrationBuilder.DropColumn(
                name: "ExportedAt",
                table: "FinalDecision");

            migrationBuilder.DropColumn(
                name: "IsExportedToRecruitment",
                table: "FinalDecision");

            migrationBuilder.DropColumn(
                name: "ReceptionAddedAt",
                table: "FinalDecision");

            migrationBuilder.DropColumn(
                name: "SupervisorAddedAt",
                table: "FinalDecision");

            migrationBuilder.DropColumn(
                name: "SupervisorLastModifiedAt",
                table: "FinalDecision");

            migrationBuilder.DropColumn(
                name: "WorstRefractionLeft",
                table: "EyeExam");

            migrationBuilder.DropColumn(
                name: "WorstRefractionRight",
                table: "EyeExam");

            migrationBuilder.DropColumn(
                name: "ReferralReason",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "QueueNumber",
                table: "Applicants");

            migrationBuilder.AddColumn<string>(
                name: "ReferredDoctor",
                table: "Consultations",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);
        }
    }
}
