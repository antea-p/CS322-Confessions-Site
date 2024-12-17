using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS322_PZ_AnteaPrimorac5157.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Confessions",
                columns: new[] { "Id", "Title", "Content", "DateCreated", "Likes" },
                values: new object[] { 1, "My First Secret", "I've never watched Star Wars", DateTime.Now.AddDays(-5), 10 }
);

            migrationBuilder.InsertData(
                table: "Confessions",
                columns: new[] { "Id", "Title", "Content", "DateCreated", "Likes" },
                values: new object[] { 2, "Coffee Confession", "I pretend to like coffee to seem mature", DateTime.Now.AddDays(-3), 5 }
            );

            migrationBuilder.InsertData(
                table: "Confessions",
                columns: new[] { "Id", "Title", "Content", "DateCreated", "Likes" },
                values: new object[] { 3, "Programming Truth", "I copy-paste from Stack Overflow without understanding the code", DateTime.Now.AddDays(-1), 15 }
            );

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "AuthorNickname", "DateCreated", "ConfessionId" },
                values: new object[] { 1, "You're not missing much!", "MovieBuff", DateTime.Now.AddDays(-4), 1 }
            );

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "AuthorNickname", "DateCreated", "ConfessionId" },
                values: new object[] { 2, "Same here! Tea is better anyway", "CaffeineFree", DateTime.Now.AddDays(-2), 2 }
            );

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "AuthorNickname", "DateCreated", "ConfessionId" },
                values: new object[] { 3, "Don't we all? 😅", "CodeNinja", DateTime.Now.AddDays(-1), 3 }
            );

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "AuthorNickname", "DateCreated", "ConfessionId" },
                values: new object[] { 4, "At least you're honest!", "TruthSeeker", DateTime.Now.AddHours(-12), 3 }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Comments", keyColumn: "Id", keyValue: 1);
        migrationBuilder.DeleteData(table: "Comments", keyColumn: "Id", keyValue: 2);
        migrationBuilder.DeleteData(table: "Comments", keyColumn: "Id", keyValue: 3);
        migrationBuilder.DeleteData(table: "Comments", keyColumn: "Id", keyValue: 4);
        
        migrationBuilder.DeleteData(table: "Confessions", keyColumn: "Id", keyValue: 1);
        migrationBuilder.DeleteData(table: "Confessions", keyColumn: "Id", keyValue: 2);
        migrationBuilder.DeleteData(table: "Confessions", keyColumn: "Id", keyValue: 3);
        }
    }
}
