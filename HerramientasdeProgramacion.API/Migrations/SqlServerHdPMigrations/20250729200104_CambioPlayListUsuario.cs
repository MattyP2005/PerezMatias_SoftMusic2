using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HerramientasdeProgramacion.API.Migrations.SqlServerHdPMigrations
{
    /// <inheritdoc />
    public partial class CambioPlayListUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayLists_Usuario_UsuarioId",
                table: "PlayLists");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayLists_Usuario_UsuarioId1",
                table: "PlayLists");

            migrationBuilder.DropIndex(
                name: "IX_PlayLists_UsuarioId1",
                table: "PlayLists");

            migrationBuilder.DropColumn(
                name: "UsuarioId1",
                table: "PlayLists");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "PlayLists",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ArtistaId",
                table: "PlayLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PlayLists_ArtistaId",
                table: "PlayLists",
                column: "ArtistaId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayLists_Usuario_ArtistaId",
                table: "PlayLists",
                column: "ArtistaId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayLists_Usuario_UsuarioId",
                table: "PlayLists",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayLists_Usuario_ArtistaId",
                table: "PlayLists");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayLists_Usuario_UsuarioId",
                table: "PlayLists");

            migrationBuilder.DropIndex(
                name: "IX_PlayLists_ArtistaId",
                table: "PlayLists");

            migrationBuilder.DropColumn(
                name: "ArtistaId",
                table: "PlayLists");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "PlayLists",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId1",
                table: "PlayLists",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayLists_UsuarioId1",
                table: "PlayLists",
                column: "UsuarioId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayLists_Usuario_UsuarioId",
                table: "PlayLists",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayLists_Usuario_UsuarioId1",
                table: "PlayLists",
                column: "UsuarioId1",
                principalTable: "Usuario",
                principalColumn: "Id");
        }
    }
}
