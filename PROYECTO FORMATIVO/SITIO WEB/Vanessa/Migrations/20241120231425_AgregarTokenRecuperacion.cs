using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vanessa.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTokenRecuperacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proyectos_Semilleros_SemilleroId",
                table: "Proyectos");

            migrationBuilder.DropForeignKey(
                name: "FK_Semilleros_Usuarios_UsuarioId",
                table: "Semilleros");

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpiracion",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenRecuperacion",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Proyectos_Semilleros_SemilleroId",
                table: "Proyectos",
                column: "SemilleroId",
                principalTable: "Semilleros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Semilleros_Usuarios_UsuarioId",
                table: "Semilleros",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proyectos_Semilleros_SemilleroId",
                table: "Proyectos");

            migrationBuilder.DropForeignKey(
                name: "FK_Semilleros_Usuarios_UsuarioId",
                table: "Semilleros");

            migrationBuilder.DropColumn(
                name: "TokenExpiracion",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenRecuperacion",
                table: "Usuarios");

            migrationBuilder.AddForeignKey(
                name: "FK_Proyectos_Semilleros_SemilleroId",
                table: "Proyectos",
                column: "SemilleroId",
                principalTable: "Semilleros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Semilleros_Usuarios_UsuarioId",
                table: "Semilleros",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }
    }
}
