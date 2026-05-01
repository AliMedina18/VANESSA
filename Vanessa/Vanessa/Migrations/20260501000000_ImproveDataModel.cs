using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vanessa.Migrations
{
    /// <inheritdoc />
    public partial class ImproveDataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Usuario: ampliar varchar demasiado cortos ─────────────────────
            migrationBuilder.AlterColumn<string>(
                name:      "Nombre",
                table:     "Usuarios",
                type:      "character varying(100)",
                maxLength: 100,
                nullable:  false,
                oldClrType: typeof(string),
                oldType:    "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name:      "Correo",
                table:     "Usuarios",
                type:      "character varying(254)",
                maxLength: 254,
                nullable:  false,
                oldClrType: typeof(string),
                oldType:    "character varying(50)",
                oldMaxLength: 50);

            // ── Usuario: fecha de registro ────────────────────────────────────
            migrationBuilder.AddColumn<DateTime>(
                name:         "FechaCreacion",
                table:        "Usuarios",
                type:         "timestamp with time zone",
                nullable:     false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            // ── Publicacion: eliminar HoraPublicacion (redundante con FechaPublicacion) ──
            migrationBuilder.DropColumn(
                name:  "HoraPublicacion",
                table: "Publicaciones");

            // ── Semillero: corregir tipos de columna (text → varchar con límite) ─
            migrationBuilder.AlterColumn<string>(
                name:      "Nombre",
                table:     "Semilleros",
                type:      "character varying(150)",
                maxLength: 150,
                nullable:  true,
                oldClrType: typeof(string),
                oldType:    "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name:      "Area",
                table:     "Semilleros",
                type:      "character varying(100)",
                maxLength: 100,
                nullable:  true,
                oldClrType: typeof(string),
                oldType:    "text",
                oldNullable: true);

            // ── Proyecto: estado y fecha de fin ───────────────────────────────
            migrationBuilder.AddColumn<int>(
                name:         "Estado",
                table:        "Proyectos",
                type:         "integer",
                nullable:     false,
                defaultValue: 0);   // EstadoProyecto.Activo

            migrationBuilder.AddColumn<DateTime>(
                name:     "FechaFin",
                table:    "Proyectos",
                type:     "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir Proyecto
            migrationBuilder.DropColumn(name: "Estado",   table: "Proyectos");
            migrationBuilder.DropColumn(name: "FechaFin", table: "Proyectos");

            // Revertir Semillero
            migrationBuilder.AlterColumn<string>(
                name: "Area", table: "Semilleros",
                type: "text", nullable: true,
                oldClrType: typeof(string), oldType: "character varying(100)", oldMaxLength: 100, oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre", table: "Semilleros",
                type: "text", nullable: true,
                oldClrType: typeof(string), oldType: "character varying(150)", oldMaxLength: 150, oldNullable: true);

            // Restaurar HoraPublicacion
            migrationBuilder.AddColumn<TimeSpan>(
                name:     "HoraPublicacion",
                table:    "Publicaciones",
                type:     "interval",
                nullable: false,
                defaultValue: TimeSpan.Zero);

            // Revertir Usuario
            migrationBuilder.DropColumn(name: "FechaCreacion", table: "Usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "Correo", table: "Usuarios",
                type: "character varying(50)", maxLength: 50, nullable: false,
                oldClrType: typeof(string), oldType: "character varying(254)", oldMaxLength: 254);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre", table: "Usuarios",
                type: "character varying(40)", maxLength: 40, nullable: false,
                oldClrType: typeof(string), oldType: "character varying(100)", oldMaxLength: 100);
        }
    }
}
