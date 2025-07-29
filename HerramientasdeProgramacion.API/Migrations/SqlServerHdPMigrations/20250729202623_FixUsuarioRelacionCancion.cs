using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HerramientasdeProgramacion.API.Migrations.SqlServerHdPMigrations
{
    /// <inheritdoc />
    public partial class FixUsuarioRelacionCancion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Canciones_Artistas_ArtistaId",
                table: "Canciones");

            migrationBuilder.AddForeignKey(
                name: "FK_Canciones_Artistas_ArtistaId",
                table: "Canciones",
                column: "ArtistaId",
                principalTable: "Artistas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Canciones_Artistas_ArtistaId",
                table: "Canciones");

            migrationBuilder.AddForeignKey(
                name: "FK_Canciones_Artistas_ArtistaId",
                table: "Canciones",
                column: "ArtistaId",
                principalTable: "Artistas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
