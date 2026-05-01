using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Vanessa.Migrations
{
    /// <inheritdoc />
    public partial class RefactorModelo3FN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proyectos_Semilleros_SemilleroId",
                table: "Proyectos");

            migrationBuilder.DropForeignKey(
                name: "FK_Proyectos_Usuarios_UsuarioId",
                table: "Proyectos");

            migrationBuilder.DropForeignKey(
                name: "FK_Semilleros_Usuarios_UsuarioId",
                table: "Semilleros");

            migrationBuilder.DropIndex(
                name: "IX_Semilleros_UsuarioId",
                table: "Semilleros");

            migrationBuilder.DropColumn(
                name: "TokenExpiracion",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenRecuperacion",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Semilleros");

            migrationBuilder.DropColumn(
                name: "ActividadesPublicacion",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "ContenidoPublicacion",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "HoraPublicacion",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "ImagenPublicacion",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "NombrePublicacion",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "TipoPublicacion",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "EquiposInvestigacion",
                table: "Proyectos");

            migrationBuilder.RenameColumn(
                name: "Id_Publicacion",
                table: "Publicaciones",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Proyectos",
                newName: "UsuarioCoordenadorId");

            migrationBuilder.RenameIndex(
                name: "IX_Proyectos_UsuarioId",
                table: "Proyectos",
                newName: "IX_Proyectos_UsuarioCoordenadorId");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Usuarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<long>(
                name: "Documento",
                table: "Usuarios",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Usuarios",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Contraseña",
                table: "Usuarios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaOtorgamiento",
                table: "UsuarioPermisos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "UsuarioPermisos",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Semilleros",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Imagen",
                table: "Semilleros",
                type: "character varying(260)",
                maxLength: 260,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Semilleros",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Area",
                table: "Semilleros",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Semilleros",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "Semilleros",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Semilleros",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UsuarioCoordinadorId",
                table: "Semilleros",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "LugarPublicacion",
                table: "Publicaciones",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contenido",
                table: "Publicaciones",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "Publicaciones",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Publicaciones",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Imagen",
                table: "Publicaciones",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoPublicacionId",
                table: "Publicaciones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "Publicaciones",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Proyectos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentoProyecto",
                table: "Proyectos",
                type: "character varying(260)",
                maxLength: 260,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Proyectos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "Proyectos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Proyectos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFin",
                table: "Proyectos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProyectoMiembros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProyectoId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    RolMiembro = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaIncorporacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaDesvinculacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProyectoMiembros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProyectoMiembros_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProyectoMiembros_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicacionAdjuntos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicacionId = table.Column<int>(type: "integer", nullable: false),
                    RutaArchivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TipoArchivo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Orden = table.Column<int>(type: "integer", nullable: true),
                    FechaAgregado = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicacionAdjuntos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicacionAdjuntos_Publicaciones_PublicacionId",
                        column: x => x.PublicacionId,
                        principalTable: "Publicaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolPermisos",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "integer", nullable: false),
                    PermisoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolPermisos", x => new { x.RolId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_RolPermisos_Permisos_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "Permisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolPermisos_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TiposPublicacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposPublicacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioAuditorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    TokenRecuperacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TokenExpiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaCreacionToken = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaUltimoLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioAuditorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioAuditorias_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Documento",
                table: "Usuarios",
                column: "Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Semilleros_Activo_FechaCreacion",
                table: "Semilleros",
                columns: new[] { "Activo", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Semilleros_Nombre",
                table: "Semilleros",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Semilleros_UsuarioCoordinadorId",
                table: "Semilleros",
                column: "UsuarioCoordinadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Publicaciones_FechaPublicacion",
                table: "Publicaciones",
                column: "FechaPublicacion");

            migrationBuilder.CreateIndex(
                name: "IX_Publicaciones_TipoPublicacionId",
                table: "Publicaciones",
                column: "TipoPublicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_SemilleroId_Estado",
                table: "Proyectos",
                columns: new[] { "SemilleroId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoMiembros_ProyectoId_RolMiembro",
                table: "ProyectoMiembros",
                columns: new[] { "ProyectoId", "RolMiembro" });

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoMiembros_ProyectoId_UsuarioId",
                table: "ProyectoMiembros",
                columns: new[] { "ProyectoId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoMiembros_UsuarioId",
                table: "ProyectoMiembros",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicacionAdjuntos_PublicacionId",
                table: "PublicacionAdjuntos",
                column: "PublicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_PermisoId",
                table: "RolPermisos",
                column: "PermisoId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioAuditorias_UsuarioId",
                table: "UsuarioAuditorias",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Proyectos_Semilleros_SemilleroId",
                table: "Proyectos",
                column: "SemilleroId",
                principalTable: "Semilleros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Proyectos_Usuarios_UsuarioCoordenadorId",
                table: "Proyectos",
                column: "UsuarioCoordenadorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Publicaciones_TiposPublicacion_TipoPublicacionId",
                table: "Publicaciones",
                column: "TipoPublicacionId",
                principalTable: "TiposPublicacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Semilleros_Usuarios_UsuarioCoordinadorId",
                table: "Semilleros",
                column: "UsuarioCoordinadorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proyectos_Semilleros_SemilleroId",
                table: "Proyectos");

            migrationBuilder.DropForeignKey(
                name: "FK_Proyectos_Usuarios_UsuarioCoordenadorId",
                table: "Proyectos");

            migrationBuilder.DropForeignKey(
                name: "FK_Publicaciones_TiposPublicacion_TipoPublicacionId",
                table: "Publicaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Semilleros_Usuarios_UsuarioCoordinadorId",
                table: "Semilleros");

            migrationBuilder.DropTable(
                name: "ProyectoMiembros");

            migrationBuilder.DropTable(
                name: "PublicacionAdjuntos");

            migrationBuilder.DropTable(
                name: "RolPermisos");

            migrationBuilder.DropTable(
                name: "TiposPublicacion");

            migrationBuilder.DropTable(
                name: "UsuarioAuditorias");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Documento",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Semilleros_Activo_FechaCreacion",
                table: "Semilleros");

            migrationBuilder.DropIndex(
                name: "IX_Semilleros_Nombre",
                table: "Semilleros");

            migrationBuilder.DropIndex(
                name: "IX_Semilleros_UsuarioCoordinadorId",
                table: "Semilleros");

            migrationBuilder.DropIndex(
                name: "IX_Publicaciones_FechaPublicacion",
                table: "Publicaciones");

            migrationBuilder.DropIndex(
                name: "IX_Publicaciones_TipoPublicacionId",
                table: "Publicaciones");

            migrationBuilder.DropIndex(
                name: "IX_Proyectos_SemilleroId_Estado",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FechaOtorgamiento",
                table: "UsuarioPermisos");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "UsuarioPermisos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Semilleros");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Semilleros");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Semilleros");

            migrationBuilder.DropColumn(
                name: "UsuarioCoordinadorId",
                table: "Semilleros");

            migrationBuilder.DropColumn(
                name: "Contenido",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "Imagen",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "TipoPublicacionId",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "FechaFin",
                table: "Proyectos");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Publicaciones",
                newName: "Id_Publicacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCoordenadorId",
                table: "Proyectos",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Proyectos_UsuarioCoordenadorId",
                table: "Proyectos",
                newName: "IX_Proyectos_UsuarioId");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Usuarios",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "Documento",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Usuarios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(254)",
                oldMaxLength: 254);

            migrationBuilder.AlterColumn<string>(
                name: "Contraseña",
                table: "Usuarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpiracion",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenRecuperacion",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Semilleros",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Imagen",
                table: "Semilleros",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(260)",
                oldMaxLength: 260,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Semilleros",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Area",
                table: "Semilleros",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Semilleros",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LugarPublicacion",
                table: "Publicaciones",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActividadesPublicacion",
                table: "Publicaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContenidoPublicacion",
                table: "Publicaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraPublicacion",
                table: "Publicaciones",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "ImagenPublicacion",
                table: "Publicaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombrePublicacion",
                table: "Publicaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoPublicacion",
                table: "Publicaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Proyectos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentoProyecto",
                table: "Proyectos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(260)",
                oldMaxLength: 260,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EquiposInvestigacion",
                table: "Proyectos",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Semilleros_UsuarioId",
                table: "Semilleros",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Proyectos_Semilleros_SemilleroId",
                table: "Proyectos",
                column: "SemilleroId",
                principalTable: "Semilleros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Proyectos_Usuarios_UsuarioId",
                table: "Proyectos",
                column: "UsuarioId",
                principalTable: "Usuarios",
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
