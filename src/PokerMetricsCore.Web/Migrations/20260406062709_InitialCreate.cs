using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PokerMetricsCore.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DEFINICAO_TORNEIO",
                columns: table => new
                {
                    DefinicaoTorneioId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    BuyIn = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    HorarioComeco = table.Column<TimeOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DEFINICAO_TORNEIO", x => x.DefinicaoTorneioId);
                });

            migrationBuilder.CreateTable(
                name: "PLAYER",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PLAYER", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "ARQUIVO",
                columns: table => new
                {
                    ArquivoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomeOriginalArquivo = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    DataUploadUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ARQUIVO", x => x.ArquivoId);
                    table.ForeignKey(
                        name: "FK_ARQUIVO_PLAYER_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "PLAYER",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TORNEIO_JOGADO",
                columns: table => new
                {
                    TorneioJogadoId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReferenceId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DataComeco = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalBuyIn = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    TotalPayout = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    NetResult = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    DefinicaoTorneioId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TORNEIO_JOGADO", x => x.TorneioJogadoId);
                    table.ForeignKey(
                        name: "FK_TORNEIO_JOGADO_DEFINICAO_TORNEIO_DefinicaoTorneioId",
                        column: x => x.DefinicaoTorneioId,
                        principalTable: "DEFINICAO_TORNEIO",
                        principalColumn: "DefinicaoTorneioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TORNEIO_JOGADO_PLAYER_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "PLAYER",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TRANSACAO",
                columns: table => new
                {
                    TransacaoId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ReferenceId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ValorMonetario = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    Pontos = table.Column<decimal>(type: "decimal(10, 2)", nullable: false),
                    ArquivoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSACAO", x => x.TransacaoId);
                    table.ForeignKey(
                        name: "FK_TRANSACAO_ARQUIVO_ArquivoId",
                        column: x => x.ArquivoId,
                        principalTable: "ARQUIVO",
                        principalColumn: "ArquivoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DEFINICAO_TORNEIO",
                columns: new[] { "DefinicaoTorneioId", "BuyIn", "HorarioComeco", "Nome" },
                values: new object[,]
                {
                    { 1, 5.50m, new TimeOnly(23, 34, 0), "MICRO ROLLER" },
                    { 2, 5.50m, new TimeOnly(1, 54, 0), "MICRO NIGHTLY" },
                    { 3, 11.00m, new TimeOnly(0, 20, 0), "LUCKY 7s" },
                    { 4, 11.00m, new TimeOnly(14, 30, 0), "CRAZY 8s" },
                    { 5, 11.00m, new TimeOnly(23, 10, 0), "CRAZY KO" },
                    { 6, 16.50m, new TimeOnly(22, 34, 0), "LOW ROLLER" },
                    { 7, 16.50m, new TimeOnly(2, 10, 0), "CRAZY 8s MADRUGADA" },
                    { 8, 9.90m, new TimeOnly(3, 40, 0), "MINOR NINER" },
                    { 9, 9.90m, new TimeOnly(15, 30, 0), "EARLY NINER" },
                    { 10, 4.40m, new TimeOnly(1, 10, 0), "MICRO MONSTER" },
                    { 11, 55.00m, new TimeOnly(2, 30, 0), "NIGHTLY" },
                    { 12, 0.00m, new TimeOnly(0, 0, 0), "FREEROLL" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ARQUIVO_PlayerId",
                table: "ARQUIVO",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TORNEIO_JOGADO_DefinicaoTorneioId",
                table: "TORNEIO_JOGADO",
                column: "DefinicaoTorneioId");

            migrationBuilder.CreateIndex(
                name: "IX_TORNEIO_JOGADO_PlayerId",
                table: "TORNEIO_JOGADO",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TORNEIO_JOGADO_ReferenceId",
                table: "TORNEIO_JOGADO",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_TRANSACAO_ArquivoId",
                table: "TRANSACAO",
                column: "ArquivoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TORNEIO_JOGADO");

            migrationBuilder.DropTable(
                name: "TRANSACAO");

            migrationBuilder.DropTable(
                name: "DEFINICAO_TORNEIO");

            migrationBuilder.DropTable(
                name: "ARQUIVO");

            migrationBuilder.DropTable(
                name: "PLAYER");
        }
    }
}
