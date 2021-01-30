using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AutonoFit.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    ClientId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    BirthMonth = table.Column<int>(nullable: false),
                    BirthDay = table.Column<int>(nullable: false),
                    BirthYear = table.Column<int>(nullable: false),
                    Age = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    EquipmentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.EquipmentId);
                });

            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    GoalId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.GoalId);
                });

            migrationBuilder.CreateTable(
                name: "ClientExercise",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(nullable: false),
                    ExerciseId = table.Column<int>(nullable: false),
                    WeekId = table.Column<int>(nullable: true),
                    WorkoutId = table.Column<int>(nullable: false),
                    RPE = table.Column<int>(nullable: false),
                    Reps = table.Column<int>(nullable: false),
                    DeltaRPECount = table.Column<int>(nullable: false),
                    LastPerformed = table.Column<int>(nullable: true),
                    TimeSinceLast = table.Column<TimeSpan>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientExercise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientExercise_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientEquipment",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipmentId = table.Column<int>(nullable: false),
                    ClientId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientEquipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientEquipment_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientEquipment_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientWeek",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(nullable: false),
                    WeekStart = table.Column<DateTime>(nullable: false),
                    WeekEnd = table.Column<DateTime>(nullable: false),
                    WorkoutsExpected = table.Column<int>(nullable: false),
                    WorkoutsCompleted = table.Column<int>(nullable: false),
                    Completed = table.Column<bool>(nullable: false),
                    MostRecentWorkoutId = table.Column<int>(nullable: true),
                    LastWeekId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientWeek", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientWeek_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientWorkout",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeekId = table.Column<int>(nullable: true),
                    Completed = table.Column<bool>(nullable: false),
                    DatePerformed = table.Column<DateTime>(nullable: true),
                    OverallDifficultyRating = table.Column<int>(nullable: true),
                    LastWorkoutId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientWorkout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientWorkout_ClientWeek_WeekId",
                        column: x => x.WeekId,
                        principalTable: "ClientWeek",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PeriodGoals",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GoalId = table.Column<int>(nullable: false),
                    WeekId = table.Column<int>(nullable: false),
                    WorkoutId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodGoals_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "GoalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeriodGoals_ClientWorkout_WorkoutId",
                        column: x => x.WorkoutId,
                        principalTable: "ClientWorkout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Equipment",
                columns: new[] { "EquipmentId", "Name" },
                values: new object[,]
                {
                    { 1, "Barbell" },
                    { 2, "SZ-Bar" },
                    { 3, "Dumbbell" },
                    { 4, "Gym mat" },
                    { 5, "Swiss Ball" },
                    { 6, "Pull-up Bar" },
                    { 8, "Bench" },
                    { 9, "Incline Bench" },
                    { 10, "Kettlebell" }
                });

            migrationBuilder.InsertData(
                table: "Goals",
                columns: new[] { "GoalId", "Name" },
                values: new object[,]
                {
                    { 1, "Strength" },
                    { 2, "Hypertrophy" },
                    { 3, "Muscular Endurance" },
                    { 4, "Cardiovascular Endurance" },
                    { 5, "Weightloss" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientEquipment_ClientId",
                table: "ClientEquipment",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientEquipment_EquipmentId",
                table: "ClientEquipment",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientExercise_ClientId",
                table: "ClientExercise",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientWeek_ClientId",
                table: "ClientWeek",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientWeek_MostRecentWorkoutId",
                table: "ClientWeek",
                column: "MostRecentWorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientWorkout_WeekId",
                table: "ClientWorkout",
                column: "WeekId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodGoals_GoalId",
                table: "PeriodGoals",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodGoals_WorkoutId",
                table: "PeriodGoals",
                column: "WorkoutId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientWeek_ClientWorkout_MostRecentWorkoutId",
                table: "ClientWeek",
                column: "MostRecentWorkoutId",
                principalTable: "ClientWorkout",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientWeek_Client_ClientId",
                table: "ClientWeek");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientWeek_ClientWorkout_MostRecentWorkoutId",
                table: "ClientWeek");

            migrationBuilder.DropTable(
                name: "ClientEquipment");

            migrationBuilder.DropTable(
                name: "ClientExercise");

            migrationBuilder.DropTable(
                name: "PeriodGoals");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropTable(
                name: "ClientWorkout");

            migrationBuilder.DropTable(
                name: "ClientWeek");
        }
    }
}
