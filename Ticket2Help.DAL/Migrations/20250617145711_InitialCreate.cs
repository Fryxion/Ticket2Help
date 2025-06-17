using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticket2Help.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CodigoColaborador = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    DataAtendimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstadoAtendimento = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    Equipamento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Avaria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescricaoReparacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PecasSubstituidas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomeSoftware = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescricaoNecessidade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescricaoIntervencao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");
        }
    }
}
