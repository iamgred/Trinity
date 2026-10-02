using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeamServer.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Payloads",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ListenerID = table.Column<int>(type: "integer", nullable: false),
                    ProfileID = table.Column<int>(type: "integer", nullable: false),
                    UUID = table.Column<string>(type: "text", nullable: false),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    Architecture = table.Column<int>(type: "integer", nullable: false),
                    PayloadType = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payloads", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Protocols",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Protocols", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TaskStatus",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskStatus", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Listeners",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProtocolID = table.Column<int>(type: "integer", nullable: false),
                    Config = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listeners", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Listeners_Protocols_ProtocolID",
                        column: x => x.ProtocolID,
                        principalTable: "Protocols",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgentID = table.Column<int>(type: "integer", nullable: false),
                    CommandType = table.Column<byte>(type: "smallint", nullable: false),
                    Command = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    StatusID = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tasks_TaskStatus_StatusID",
                        column: x => x.StatusID,
                        principalTable: "TaskStatus",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CheckInUUID = table.Column<string>(type: "text", nullable: false),
                    CampaignID = table.Column<int>(type: "integer", nullable: false),
                    ListenerID = table.Column<int>(type: "integer", nullable: false),
                    PayloadID = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    ProcesseName = table.Column<string>(type: "text", nullable: false),
                    Architecure = table.Column<string>(type: "text", nullable: false),
                    ProcessPID = table.Column<int>(type: "integer", nullable: false),
                    Integrity = table.Column<string>(type: "text", nullable: false),
                    LastCheckIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sleep = table.Column<int>(type: "integer", nullable: false),
                    Jitter = table.Column<int>(type: "integer", nullable: false),
                    ExternalIP = table.Column<string>(type: "text", nullable: false),
                    InternalIP = table.Column<string>(type: "text", nullable: false),
                    AES256Key = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Agents_Campaigns_CampaignID",
                        column: x => x.CampaignID,
                        principalTable: "Campaigns",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Agents_Listeners_ListenerID",
                        column: x => x.ListenerID,
                        principalTable: "Listeners",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Agents_Payloads_PayloadID",
                        column: x => x.PayloadID,
                        principalTable: "Payloads",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskResult",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskID = table.Column<int>(type: "integer", nullable: false),
                    isSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    Response = table.Column<JsonDocument>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskResult", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TaskResult_Tasks_TaskID",
                        column: x => x.TaskID,
                        principalTable: "Tasks",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Campaigns",
                columns: new[] { "ID", "Name" },
                values: new object[] { 1, "test" });

            migrationBuilder.InsertData(
                table: "Payloads",
                columns: new[] { "ID", "Architecture", "CreatedAt", "FileName", "ListenerID", "PayloadType", "Platform", "ProfileID", "UUID" },
                values: new object[] { 1, 0, new DateTime(2021, 2, 15, 2, 2, 2, 0, DateTimeKind.Utc), "test", 1, 0, 2, 1, "test" });

            migrationBuilder.InsertData(
                table: "Protocols",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { 1, "HTTP" },
                    { 2, "SMB" },
                    { 3, "TCP" }
                });

            migrationBuilder.InsertData(
                table: "TaskStatus",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { 1, "Queued" },
                    { 2, "Pending" },
                    { 3, "Running" },
                    { 4, "Successful" },
                    { 5, "Failure" }
                });

            migrationBuilder.InsertData(
                table: "Listeners",
                columns: new[] { "ID", "Config", "Name", "ProtocolID" },
                values: new object[] { 1, null, "test_http", 1 });

            migrationBuilder.InsertData(
                table: "Agents",
                columns: new[] { "ID", "AES256Key", "Architecure", "CampaignID", "CheckInUUID", "ExternalIP", "Integrity", "InternalIP", "Jitter", "LastCheckIn", "ListenerID", "PayloadID", "ProcessPID", "ProcesseName", "Sleep", "Username" },
                values: new object[] { 1, "testKey", "x86", 1, "test", "dsfdsf", "stes", "dsf", 10, new DateTime(2021, 2, 15, 2, 2, 2, 0, DateTimeKind.Utc), 1, 1, 2000, "test", 5000, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_CampaignID",
                table: "Agents",
                column: "CampaignID");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_ListenerID",
                table: "Agents",
                column: "ListenerID");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_PayloadID",
                table: "Agents",
                column: "PayloadID");

            migrationBuilder.CreateIndex(
                name: "IX_Listeners_ProtocolID",
                table: "Listeners",
                column: "ProtocolID");

            migrationBuilder.CreateIndex(
                name: "IX_TaskResult_TaskID",
                table: "TaskResult",
                column: "TaskID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StatusID",
                table: "Tasks",
                column: "StatusID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "TaskResult");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "Listeners");

            migrationBuilder.DropTable(
                name: "Payloads");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Protocols");

            migrationBuilder.DropTable(
                name: "TaskStatus");
        }
    }
}
