using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControlOverWeb.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cihazlar",
                columns: table => new
                {
                    CihazID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CihazAdi = table.Column<string>(type: "text", nullable: false),
                    CihazToken = table.Column<string>(type: "text", nullable: false),
                    SonGorulme = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cihazlar", x => x.CihazID);
                });

            migrationBuilder.CreateTable(
                name: "KomutKuyrugu",
                columns: table => new
                {
                    KomutID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KomutIcerigi = table.Column<string>(type: "text", nullable: false),
                    Durum = table.Column<string>(type: "text", nullable: false),
                    OlusturmaZamani = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IslemeAlmaZamani = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TamamlanmaZamani = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CihazID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KomutKuyrugu", x => x.KomutID);
                    table.ForeignKey(
                        name: "FK_KomutKuyrugu_Cihazlar_CihazID",
                        column: x => x.CihazID,
                        principalTable: "Cihazlar",
                        principalColumn: "CihazID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KomutKuyrugu_CihazID",
                table: "KomutKuyrugu",
                column: "CihazID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KomutKuyrugu");

            migrationBuilder.DropTable(
                name: "Cihazlar");
        }
    }
}
