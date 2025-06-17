using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticket2Help.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarEstrutura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PecasSubstituidas",
                table: "Tickets",
                newName: "SoftwareName");

            migrationBuilder.RenameColumn(
                name: "NomeSoftware",
                table: "Tickets",
                newName: "ReplacementParts");

            migrationBuilder.RenameColumn(
                name: "EstadoAtendimento",
                table: "Tickets",
                newName: "AttendanceStatus");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "Tickets",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Equipamento",
                table: "Tickets",
                newName: "RepairDescription");

            migrationBuilder.RenameColumn(
                name: "DescricaoReparacao",
                table: "Tickets",
                newName: "NecessityDescription");

            migrationBuilder.RenameColumn(
                name: "DescricaoNecessidade",
                table: "Tickets",
                newName: "Malfunction");

            migrationBuilder.RenameColumn(
                name: "DescricaoIntervencao",
                table: "Tickets",
                newName: "InterventionDescription");

            migrationBuilder.RenameColumn(
                name: "DataCriacao",
                table: "Tickets",
                newName: "CreationDate");

            migrationBuilder.RenameColumn(
                name: "DataAtendimento",
                table: "Tickets",
                newName: "AttendanceDate");

            migrationBuilder.RenameColumn(
                name: "CodigoColaborador",
                table: "Tickets",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "Avaria",
                table: "Tickets",
                newName: "Equipment");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Tickets",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "SoftwareName",
                table: "Tickets",
                newName: "PecasSubstituidas");

            migrationBuilder.RenameColumn(
                name: "ReplacementParts",
                table: "Tickets",
                newName: "NomeSoftware");

            migrationBuilder.RenameColumn(
                name: "RepairDescription",
                table: "Tickets",
                newName: "Equipamento");

            migrationBuilder.RenameColumn(
                name: "NecessityDescription",
                table: "Tickets",
                newName: "DescricaoReparacao");

            migrationBuilder.RenameColumn(
                name: "Malfunction",
                table: "Tickets",
                newName: "DescricaoNecessidade");

            migrationBuilder.RenameColumn(
                name: "InterventionDescription",
                table: "Tickets",
                newName: "DescricaoIntervencao");

            migrationBuilder.RenameColumn(
                name: "Equipment",
                table: "Tickets",
                newName: "Avaria");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Tickets",
                newName: "CodigoColaborador");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "Tickets",
                newName: "DataCriacao");

            migrationBuilder.RenameColumn(
                name: "AttendanceStatus",
                table: "Tickets",
                newName: "EstadoAtendimento");

            migrationBuilder.RenameColumn(
                name: "AttendanceDate",
                table: "Tickets",
                newName: "DataAtendimento");
        }
    }
}
