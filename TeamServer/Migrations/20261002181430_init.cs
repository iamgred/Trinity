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
                name: "Admins",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CampaignBridges",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignID = table.Column<int>(type: "integer", nullable: false),
                    OperatorID = table.Column<int>(type: "integer", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignBridges", x => x.ID);
                });

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
                name: "Hosts",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgentID = table.Column<int>(type: "integer", nullable: false),
                    HostName = table.Column<string>(type: "text", nullable: false),
                    OS = table.Column<string>(type: "text", nullable: false),
                    Motherboard = table.Column<string>(type: "text", nullable: false),
                    RAM = table.Column<int>(type: "integer", nullable: false),
                    DiskSize = table.Column<double>(type: "double precision", nullable: false),
                    FreeDisk = table.Column<double>(type: "double precision", nullable: false),
                    CPUCount = table.Column<int>(type: "integer", nullable: false),
                    MACAddress = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hosts", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Payloads",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PayloadUUID = table.Column<string>(type: "text", nullable: false),
                    ListenerID = table.Column<int>(type: "integer", nullable: false),
                    CampaignID = table.Column<int>(type: "integer", nullable: false),
                    CreatedByOperatorID = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    Architecture = table.Column<int>(type: "integer", nullable: false),
                    RetryStrategy = table.Column<string>(type: "text", nullable: false),
                    PayloadType = table.Column<int>(type: "integer", nullable: false),
                    AES256KEY = table.Column<string>(type: "text", nullable: false),
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
                    Config = table.Column<JsonDocument>(type: "jsonb", nullable: false)
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
                name: "IX_CampaignBridges_CampaignID_OperatorID",
                table: "CampaignBridges",
                columns: new[] { "CampaignID", "OperatorID" },
                unique: true);

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
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "CampaignBridges");

            migrationBuilder.DropTable(
                name: "Hosts");

            migrationBuilder.DropTable(
                name: "Operators");

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
