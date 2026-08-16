using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RiskPulse.Migrations
{
    /// <inheritdoc />
    public partial class UserPermissionControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "riskpulse");

            migrationBuilder.CreateTable(
                name: "Groups",
                schema: "riskpulse",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupDesc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.GroupId);
                });

            migrationBuilder.CreateTable(
                name: "KriThresholdColors",
                schema: "riskpulse",
                columns: table => new
                {
                    ColorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ColorDesc = table.Column<string>(type: "text", nullable: false),
                    HexCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KriThresholdColors", x => x.ColorId);
                });

            migrationBuilder.CreateTable(
                name: "KriThresholdGroups",
                schema: "riskpulse",
                columns: table => new
                {
                    KriThresholdGroupId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KriThresholdGroupDesc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KriThresholdGroups", x => x.KriThresholdGroupId);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "riskpulse",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PermissionDesc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                schema: "riskpulse",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitCode = table.Column<string>(type: "text", nullable: false),
                    UnitType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UnitDesc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.UnitId);
                });

            migrationBuilder.CreateTable(
                name: "KriHeaders",
                schema: "riskpulse",
                columns: table => new
                {
                    KriHeaderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KriHeaderDesc = table.Column<string>(type: "text", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    KriStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KriHeaders", x => x.KriHeaderId);
                    table.ForeignKey(
                        name: "FK_KriHeaders_Groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "riskpulse",
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SaqHeaders",
                schema: "riskpulse",
                columns: table => new
                {
                    SaqHeaderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaqDesc = table.Column<string>(type: "text", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    SaqStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaqHeaders", x => x.SaqHeaderId);
                    table.ForeignKey(
                        name: "FK_SaqHeaders_Groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "riskpulse",
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KriThresholds",
                schema: "riskpulse",
                columns: table => new
                {
                    KriThresholdId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KriThresholdGroupId = table.Column<int>(type: "integer", nullable: false),
                    ColorId = table.Column<int>(type: "integer", nullable: false),
                    MinValue = table.Column<int>(type: "integer", nullable: false),
                    MaxValue = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KriThresholds", x => x.KriThresholdId);
                    table.ForeignKey(
                        name: "FK_KriThresholds_KriThresholdColors_ColorId",
                        column: x => x.ColorId,
                        principalSchema: "riskpulse",
                        principalTable: "KriThresholdColors",
                        principalColumn: "ColorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KriThresholds_KriThresholdGroups_KriThresholdGroupId",
                        column: x => x.KriThresholdGroupId,
                        principalSchema: "riskpulse",
                        principalTable: "KriThresholdGroups",
                        principalColumn: "KriThresholdGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "riskpulse",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleDesc = table.Column<string>(type: "text", nullable: false),
                    DefaultPermissionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_Roles_Permissions_DefaultPermissionId",
                        column: x => x.DefaultPermissionId,
                        principalSchema: "riskpulse",
                        principalTable: "Permissions",
                        principalColumn: "PermissionId");
                });

            migrationBuilder.CreateTable(
                name: "UnitGroups",
                schema: "riskpulse",
                columns: table => new
                {
                    UnitGroupId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitGroups", x => x.UnitGroupId);
                    table.ForeignKey(
                        name: "FK_UnitGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "riskpulse",
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnitGroups_Units_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "riskpulse",
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kris",
                schema: "riskpulse",
                columns: table => new
                {
                    KriId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KriHeaderId = table.Column<int>(type: "integer", nullable: false),
                    KriDesc = table.Column<string>(type: "text", nullable: false),
                    AllowComment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    KriThresholdGroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kris", x => x.KriId);
                    table.ForeignKey(
                        name: "FK_Kris_KriHeaders_KriHeaderId",
                        column: x => x.KriHeaderId,
                        principalSchema: "riskpulse",
                        principalTable: "KriHeaders",
                        principalColumn: "KriHeaderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Kris_KriThresholdGroups_KriThresholdGroupId",
                        column: x => x.KriThresholdGroupId,
                        principalSchema: "riskpulse",
                        principalTable: "KriThresholdGroups",
                        principalColumn: "KriThresholdGroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentHeaders",
                schema: "riskpulse",
                columns: table => new
                {
                    AssessmentHeaderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssessmentName = table.Column<string>(type: "text", nullable: false),
                    AssessmentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SaqHeaderId = table.Column<int>(type: "integer", nullable: true),
                    KriHeaderId = table.Column<int>(type: "integer", nullable: true),
                    RiskRegisterHeaderId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentHeaders", x => x.AssessmentHeaderId);
                    table.ForeignKey(
                        name: "FK_AssessmentHeaders_KriHeaders_KriHeaderId",
                        column: x => x.KriHeaderId,
                        principalSchema: "riskpulse",
                        principalTable: "KriHeaders",
                        principalColumn: "KriHeaderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssessmentHeaders_SaqHeaders_SaqHeaderId",
                        column: x => x.SaqHeaderId,
                        principalSchema: "riskpulse",
                        principalTable: "SaqHeaders",
                        principalColumn: "SaqHeaderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SaqQuestions",
                schema: "riskpulse",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaqHeaderId = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AllowComment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaqQuestions", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_SaqQuestions_SaqHeaders_SaqHeaderId",
                        column: x => x.SaqHeaderId,
                        principalSchema: "riskpulse",
                        principalTable: "SaqHeaders",
                        principalColumn: "SaqHeaderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "riskpulse",
                columns: table => new
                {
                    RolePermissionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.RolePermissionId);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "riskpulse",
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "riskpulse",
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "riskpulse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "riskpulse",
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Units_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "riskpulse",
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleHeaders",
                schema: "riskpulse",
                columns: table => new
                {
                    ScheduleHeaderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssessmentHeaderId = table.Column<int>(type: "integer", nullable: false),
                    ScheduleDesc = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleHeaders", x => x.ScheduleHeaderId);
                    table.ForeignKey(
                        name: "FK_ScheduleHeaders_AssessmentHeaders_AssessmentHeaderId",
                        column: x => x.AssessmentHeaderId,
                        principalSchema: "riskpulse",
                        principalTable: "AssessmentHeaders",
                        principalColumn: "AssessmentHeaderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaqQuestionOptions",
                schema: "riskpulse",
                columns: table => new
                {
                    OptionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    OptionText = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaqQuestionOptions", x => x.OptionId);
                    table.ForeignKey(
                        name: "FK_SaqQuestionOptions_SaqQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "riskpulse",
                        principalTable: "SaqQuestions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentHeaders_KriHeaderId",
                schema: "riskpulse",
                table: "AssessmentHeaders",
                column: "KriHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentHeaders_SaqHeaderId",
                schema: "riskpulse",
                table: "AssessmentHeaders",
                column: "SaqHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_KriHeaders_GroupId",
                schema: "riskpulse",
                table: "KriHeaders",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Kris_KriHeaderId",
                schema: "riskpulse",
                table: "Kris",
                column: "KriHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Kris_KriThresholdGroupId",
                schema: "riskpulse",
                table: "Kris",
                column: "KriThresholdGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_KriThresholds_ColorId",
                schema: "riskpulse",
                table: "KriThresholds",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_KriThresholds_KriThresholdGroupId",
                schema: "riskpulse",
                table: "KriThresholds",
                column: "KriThresholdGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                schema: "riskpulse",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                schema: "riskpulse",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_DefaultPermissionId",
                schema: "riskpulse",
                table: "Roles",
                column: "DefaultPermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SaqHeaders_GroupId",
                schema: "riskpulse",
                table: "SaqHeaders",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SaqQuestionOptions_QuestionId",
                schema: "riskpulse",
                table: "SaqQuestionOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_SaqQuestions_SaqHeaderId",
                schema: "riskpulse",
                table: "SaqQuestions",
                column: "SaqHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleHeaders_AssessmentHeaderId",
                schema: "riskpulse",
                table: "ScheduleHeaders",
                column: "AssessmentHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitGroups_GroupId_UnitId",
                schema: "riskpulse",
                table: "UnitGroups",
                columns: new[] { "GroupId", "UnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitGroups_UnitId",
                schema: "riskpulse",
                table: "UnitGroups",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                schema: "riskpulse",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UnitId",
                schema: "riskpulse",
                table: "Users",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kris",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "KriThresholds",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "SaqQuestionOptions",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "ScheduleHeaders",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "UnitGroups",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "KriThresholdColors",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "KriThresholdGroups",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "SaqQuestions",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "AssessmentHeaders",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "Units",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "KriHeaders",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "SaqHeaders",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "riskpulse");

            migrationBuilder.DropTable(
                name: "Groups",
                schema: "riskpulse");
        }
    }
}
