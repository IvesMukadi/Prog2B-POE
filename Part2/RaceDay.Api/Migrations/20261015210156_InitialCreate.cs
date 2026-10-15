using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Enrolment_Status",
                columns: table => new
                {
                    STATUS_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    STATUS_NAME = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrolment_Status", x => x.STATUS_ID);
                });

            migrationBuilder.CreateTable(
                name: "Event_Status",
                columns: table => new
                {
                    EVENT_STATUS_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    STATUS_NAME = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event_Status", x => x.EVENT_STATUS_ID);
                });

            migrationBuilder.CreateTable(
                name: "Event_Types",
                columns: table => new
                {
                    TYPE_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TYPE_NAME = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event_Types", x => x.TYPE_ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    USER_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EMAIL = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    PASSWORD = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    FIRST_NAME = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    LAST_NAME = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ROLE = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    PROFILE_PICTURE_URL = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.USER_ID);
                    table.CheckConstraint("CK_Users_ROLE", "[ROLE] in ('Organiser', 'Participant')");
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EVENT_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USER_ID = table.Column<int>(type: "int", nullable: false),
                    TYPE_ID = table.Column<int>(type: "int", nullable: false),
                    EVENT_STATUS_ID = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    NAME = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    EVENT_DATE = table.Column<DateTime>(type: "datetime", nullable: false),
                    LOCATION = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    DISTANCE = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    BANNER_URL = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EVENT_ID);
                    table.ForeignKey(
                        name: "FK_Events_Event_Status_EVENT_STATUS_ID",
                        column: x => x.EVENT_STATUS_ID,
                        principalTable: "Event_Status",
                        principalColumn: "EVENT_STATUS_ID");
                    table.ForeignKey(
                        name: "FK_Events_Event_Types_TYPE_ID",
                        column: x => x.TYPE_ID,
                        principalTable: "Event_Types",
                        principalColumn: "TYPE_ID");
                    table.ForeignKey(
                        name: "FK_Events_Users_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "Users",
                        principalColumn: "USER_ID");
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CATEGORY_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EVENT_ID = table.Column<int>(type: "int", nullable: false),
                    CATEGORY_NAME = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    AGE_GROUP = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CATEGORY_ID);
                    table.ForeignKey(
                        name: "FK_Categories_Events_EVENT_ID",
                        column: x => x.EVENT_ID,
                        principalTable: "Events",
                        principalColumn: "EVENT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrolments",
                columns: table => new
                {
                    ENROLMENT_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USER_ID = table.Column<int>(type: "int", nullable: false),
                    EVENT_ID = table.Column<int>(type: "int", nullable: false),
                    CATEGORY_ID = table.Column<int>(type: "int", nullable: false),
                    STATUS_ID = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrolments", x => x.ENROLMENT_ID);
                    table.ForeignKey(
                        name: "FK_Enrolments_Categories_CATEGORY_ID",
                        column: x => x.CATEGORY_ID,
                        principalTable: "Categories",
                        principalColumn: "CATEGORY_ID");
                    table.ForeignKey(
                        name: "FK_Enrolments_Enrolment_Status_STATUS_ID",
                        column: x => x.STATUS_ID,
                        principalTable: "Enrolment_Status",
                        principalColumn: "STATUS_ID");
                    table.ForeignKey(
                        name: "FK_Enrolments_Events_EVENT_ID",
                        column: x => x.EVENT_ID,
                        principalTable: "Events",
                        principalColumn: "EVENT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrolments_Users_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "Users",
                        principalColumn: "USER_ID");
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    RESULT_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ENROLMENT_ID = table.Column<int>(type: "int", nullable: false),
                    FINISH_TIME = table.Column<TimeSpan>(type: "time", nullable: false),
                    FINISHING_POSITION = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.RESULT_ID);
                    table.ForeignKey(
                        name: "FK_Results_Enrolments_ENROLMENT_ID",
                        column: x => x.ENROLMENT_ID,
                        principalTable: "Enrolments",
                        principalColumn: "ENROLMENT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_EVENT_ID_CATEGORY_NAME",
                table: "Categories",
                columns: new[] { "EVENT_ID", "CATEGORY_NAME" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrolment_Status_STATUS_NAME",
                table: "Enrolment_Status",
                column: "STATUS_NAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_CATEGORY_ID",
                table: "Enrolments",
                column: "CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_EVENT_ID",
                table: "Enrolments",
                column: "EVENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_STATUS_ID",
                table: "Enrolments",
                column: "STATUS_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_USER_ID",
                table: "Enrolments",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Event_Status_STATUS_NAME",
                table: "Event_Status",
                column: "STATUS_NAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Event_Types_TYPE_NAME",
                table: "Event_Types",
                column: "TYPE_NAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_EVENT_STATUS_ID",
                table: "Events",
                column: "EVENT_STATUS_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Events_TYPE_ID",
                table: "Events",
                column: "TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Events_USER_ID",
                table: "Events",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Results_ENROLMENT_ID",
                table: "Results",
                column: "ENROLMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_EMAIL",
                table: "Users",
                column: "EMAIL",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "Enrolments");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Enrolment_Status");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Event_Status");

            migrationBuilder.DropTable(
                name: "Event_Types");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
