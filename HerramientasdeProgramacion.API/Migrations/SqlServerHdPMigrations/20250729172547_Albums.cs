using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HerramientasdeProgramacion.API.Migrations.SqlServerHdPMigrations
{
    /// <inheritdoc />
    public partial class Albums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albumes_Usuario_ArtistaId",
                table: "Albumes");

            migrationBuilder.DropForeignKey(
                name: "FK_Albumes_Usuario_UsuarioId",
                table: "Albumes");

            migrationBuilder.DropIndex(
                name: "IX_Albumes_ArtistaId",
                table: "Albumes");

            migrationBuilder.DropColumn(
                name: "ArtistaId",
                table: "Albumes");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Albumes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId1",
                table: "Albumes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albumes_UsuarioId1",
                table: "Albumes",
                column: "UsuarioId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Albumes_Usuario_UsuarioId",
                table: "Albumes",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Albumes_Usuario_UsuarioId1",
                table: "Albumes",
                column: "UsuarioId1",
                principalTable: "Usuario",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albumes_Usuario_UsuarioId",
                table: "Albumes");

            migrationBuilder.DropForeignKey(
                name: "FK_Albumes_Usuario_UsuarioId1",
                table: "Albumes");

            migrationBuilder.DropIndex(
                name: "IX_Albumes_UsuarioId1",
                table: "Albumes");

            migrationBuilder.DropColumn(
                name: "UsuarioId1",
                table: "Albumes");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Albumes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ArtistaId",
                table: "Albumes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Albumes_ArtistaId",
                table: "Albumes",
                column: "ArtistaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Albumes_Usuario_ArtistaId",
                table: "Albumes",
                column: "ArtistaId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Albumes_Usuario_UsuarioId",
                table: "Albumes",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id");
        }
    }
}
