using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class inicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profesionales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Area = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cuit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MP = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RNP = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profesionales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoConsultorios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Direccion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoConsultorios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoDiagnosticos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoDiagnosticos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoDocumentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoDocumentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoGastos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoGastos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoModalidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoModalidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoObrasSociales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoObrasSociales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoPlanillas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoPlanillas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoPrestaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MontoSesion = table.Column<decimal>(type: "numeric", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoPrestaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoTurnos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DuracionMinutos = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoTurnos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserPasskeys",
                columns: table => new
                {
                    CredentialId = table.Column<byte[]>(type: "bytea", maxLength: 1024, nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserPasskeys", x => x.CredentialId);
                    table.ForeignKey(
                        name: "FK_AspNetUserPasskeys_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Socio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfesionalId = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Socio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Socio_Profesionales_ProfesionalId",
                        column: x => x.ProfesionalId,
                        principalTable: "Profesionales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipoDocumentoId = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documento_TipoDocumentos_TipoDocumentoId",
                        column: x => x.TipoDocumentoId,
                        principalTable: "TipoDocumentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Gasto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric", nullable: false),
                    TipoGastoId = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gasto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gasto_TipoGastos_TipoGastoId",
                        column: x => x.TipoGastoId,
                        principalTable: "TipoGastos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Turno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstadoTurno = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoTurnoId = table.Column<int>(type: "integer", nullable: false),
                    TipoConsultorioId = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turno", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turno_TipoConsultorios_TipoConsultorioId",
                        column: x => x.TipoConsultorioId,
                        principalTable: "TipoConsultorios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Turno_TipoTurnos_TipoTurnoId",
                        column: x => x.TipoTurnoId,
                        principalTable: "TipoTurnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Liquidacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FechaDesde = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaHasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NumeroLiquidacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SocioId = table.Column<int>(type: "integer", nullable: false),
                    ProfesionalId = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Liquidacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Liquidacion_Profesionales_ProfesionalId",
                        column: x => x.ProfesionalId,
                        principalTable: "Profesionales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Liquidacion_Socio_SocioId",
                        column: x => x.SocioId,
                        principalTable: "Socio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DNI = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    NumeroAfiliado = table.Column<int>(type: "integer", nullable: false),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Domicilio = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TipoObraSocialId = table.Column<int>(type: "integer", nullable: false),
                    TipoDiagnosticoId = table.Column<int>(type: "integer", nullable: false),
                    DocumentoId = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pacientes_Documento_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pacientes_TipoDiagnosticos_TipoDiagnosticoId",
                        column: x => x.TipoDiagnosticoId,
                        principalTable: "TipoDiagnosticos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pacientes_TipoObrasSociales_TipoObraSocialId",
                        column: x => x.TipoObraSocialId,
                        principalTable: "TipoObrasSociales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GastoSocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GastoId = table.Column<int>(type: "integer", nullable: false),
                    SocioId = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GastoSocio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GastoSocio_Gasto_GastoId",
                        column: x => x.GastoId,
                        principalTable: "Gasto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GastoSocio_Socio_SocioId",
                        column: x => x.SocioId,
                        principalTable: "Socio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TurnoProfesional",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TurnoId = table.Column<int>(type: "integer", nullable: false),
                    ProfesionalId = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurnoProfesional", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TurnoProfesional_Profesionales_ProfesionalId",
                        column: x => x.ProfesionalId,
                        principalTable: "Profesionales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TurnoProfesional_Turno_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "Turno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TurnoTipoPrestacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TurnoId = table.Column<int>(type: "integer", nullable: false),
                    TipoPrestacionId = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurnoTipoPrestacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TurnoTipoPrestacion_TipoPrestaciones_TipoPrestacionId",
                        column: x => x.TipoPrestacionId,
                        principalTable: "TipoPrestaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TurnoTipoPrestacion_Turno_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "Turno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetalleLiquidacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MontoFacturacion = table.Column<decimal>(type: "numeric", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "numeric", nullable: false),
                    Retencion = table.Column<decimal>(type: "numeric", nullable: false),
                    FechaTurno = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PacienteId = table.Column<int>(type: "integer", nullable: false),
                    ProfesionalId = table.Column<int>(type: "integer", nullable: false),
                    LiquidacionId = table.Column<int>(type: "integer", nullable: false),
                    TipoModalidadId = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleLiquidacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleLiquidacion_Liquidacion_LiquidacionId",
                        column: x => x.LiquidacionId,
                        principalTable: "Liquidacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleLiquidacion_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleLiquidacion_Profesionales_ProfesionalId",
                        column: x => x.ProfesionalId,
                        principalTable: "Profesionales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleLiquidacion_TipoModalidades_TipoModalidadId",
                        column: x => x.TipoModalidadId,
                        principalTable: "TipoModalidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TurnoPaciente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TurnoId = table.Column<int>(type: "integer", nullable: false),
                    PacienteId = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurnoPaciente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TurnoPaciente_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TurnoPaciente_Turno_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "Turno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserPasskeys_UserId",
                table: "AspNetUserPasskeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetalleLiquidacion_LiquidacionId",
                table: "DetalleLiquidacion",
                column: "LiquidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleLiquidacion_PacienteId",
                table: "DetalleLiquidacion",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleLiquidacion_ProfesionalId",
                table: "DetalleLiquidacion",
                column: "ProfesionalId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleLiquidacion_TipoModalidadId",
                table: "DetalleLiquidacion",
                column: "TipoModalidadId");

            migrationBuilder.CreateIndex(
                name: "IX_Documento_TipoDocumentoId",
                table: "Documento",
                column: "TipoDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Gasto_TipoGastoId",
                table: "Gasto",
                column: "TipoGastoId");

            migrationBuilder.CreateIndex(
                name: "IX_GastoSocio_GastoId",
                table: "GastoSocio",
                column: "GastoId");

            migrationBuilder.CreateIndex(
                name: "IX_GastoSocio_SocioId",
                table: "GastoSocio",
                column: "SocioId");

            migrationBuilder.CreateIndex(
                name: "IX_Liquidacion_ProfesionalId",
                table: "Liquidacion",
                column: "ProfesionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Liquidacion_SocioId",
                table: "Liquidacion",
                column: "SocioId");

            migrationBuilder.CreateIndex(
                name: "NumeroLiquidacion_UQ",
                table: "Liquidacion",
                column: "NumeroLiquidacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "DNI_UQ",
                table: "Pacientes",
                column: "DNI",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_DocumentoId",
                table: "Pacientes",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_TipoDiagnosticoId",
                table: "Pacientes",
                column: "TipoDiagnosticoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_TipoObraSocialId",
                table: "Pacientes",
                column: "TipoObraSocialId");

            migrationBuilder.CreateIndex(
                name: "NumeroAfiliado_UQ",
                table: "Pacientes",
                column: "NumeroAfiliado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Profesional_Cuit_UQ",
                table: "Profesionales",
                column: "Cuit",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Profesional_MP_UQ",
                table: "Profesionales",
                column: "MP",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Profesional_RNP_UQ",
                table: "Profesionales",
                column: "RNP",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ProfesionalId_UQ",
                table: "Socio",
                column: "ProfesionalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "TipoConsultorio_Tipo_UQ",
                table: "TipoConsultorios",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "TipoDiagnostico_Tipo_UQ",
                table: "TipoDiagnosticos",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "TipoDocumento_Tipo_UQ",
                table: "TipoDocumentos",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "TipoGasto_Tipo_UQ",
                table: "TipoGastos",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "TipoModalidad_Tipo_UQ",
                table: "TipoModalidades",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "TipoObraSocial_Tipo_UQ",
                table: "TipoObrasSociales",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "TipoPlanilla_Tipo_UQ",
                table: "TipoPlanillas",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "TipoPrestacion_Cod_UQ",
                table: "TipoPrestaciones",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "TipoPrestacion_Tipo_UQ",
                table: "TipoPrestaciones",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "TipoTurno_Tipo_UQ",
                table: "TipoTurnos",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Turno_TipoConsultorioId",
                table: "Turno",
                column: "TipoConsultorioId");

            migrationBuilder.CreateIndex(
                name: "IX_Turno_TipoTurnoId",
                table: "Turno",
                column: "TipoTurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnoPaciente_PacienteId",
                table: "TurnoPaciente",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnoPaciente_TurnoId",
                table: "TurnoPaciente",
                column: "TurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnoProfesional_ProfesionalId",
                table: "TurnoProfesional",
                column: "ProfesionalId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnoProfesional_TurnoId",
                table: "TurnoProfesional",
                column: "TurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnoTipoPrestacion_TipoPrestacionId",
                table: "TurnoTipoPrestacion",
                column: "TipoPrestacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnoTipoPrestacion_TurnoId",
                table: "TurnoTipoPrestacion",
                column: "TurnoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserPasskeys");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DetalleLiquidacion");

            migrationBuilder.DropTable(
                name: "GastoSocio");

            migrationBuilder.DropTable(
                name: "TipoPlanillas");

            migrationBuilder.DropTable(
                name: "TurnoPaciente");

            migrationBuilder.DropTable(
                name: "TurnoProfesional");

            migrationBuilder.DropTable(
                name: "TurnoTipoPrestacion");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Liquidacion");

            migrationBuilder.DropTable(
                name: "TipoModalidades");

            migrationBuilder.DropTable(
                name: "Gasto");

            migrationBuilder.DropTable(
                name: "Pacientes");

            migrationBuilder.DropTable(
                name: "TipoPrestaciones");

            migrationBuilder.DropTable(
                name: "Turno");

            migrationBuilder.DropTable(
                name: "Socio");

            migrationBuilder.DropTable(
                name: "TipoGastos");

            migrationBuilder.DropTable(
                name: "Documento");

            migrationBuilder.DropTable(
                name: "TipoDiagnosticos");

            migrationBuilder.DropTable(
                name: "TipoObrasSociales");

            migrationBuilder.DropTable(
                name: "TipoConsultorios");

            migrationBuilder.DropTable(
                name: "TipoTurnos");

            migrationBuilder.DropTable(
                name: "Profesionales");

            migrationBuilder.DropTable(
                name: "TipoDocumentos");
        }
    }
}
