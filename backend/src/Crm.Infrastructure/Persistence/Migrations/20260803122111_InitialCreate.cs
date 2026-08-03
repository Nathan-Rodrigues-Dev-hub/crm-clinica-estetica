using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clinicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinicas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "logs_auditoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicaId = table.Column<Guid>(type: "uuid", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    Acao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntidadeTipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntidadeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Detalhe = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DataHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs_auditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pacientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    CpfPendente = table.Column<bool>(type: "boolean", nullable: false),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DataNascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    Sexo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Endereco = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RedeSocial = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Origem = table.Column<int>(type: "integer", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pacientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pacientes_Clinicas_ClinicaId",
                        column: x => x.ClinicaId,
                        principalTable: "Clinicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "procedimentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DuracaoMinutos = table.Column<int>(type: "integer", nullable: false),
                    ValorReferencia = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_procedimentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_procedimentos_Clinicas_ClinicaId",
                        column: x => x.ClinicaId,
                        principalTable: "Clinicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SenhaHash = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Papel = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usuarios_Clinicas_ClinicaId",
                        column: x => x.ClinicaId,
                        principalTable: "Clinicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "anamnese_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TelefoneDestino = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PacienteId = table.Column<Guid>(type: "uuid", nullable: true),
                    Expiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Usado = table.Column<bool>(type: "boolean", nullable: false),
                    UsadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_anamnese_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_anamnese_links_pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "anamneses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PacienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataPreenchimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Peso = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Altura = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    Imc = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    PressaoArterial = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MotivoEstetico = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AceiteDeclaracao = table.Column<bool>(type: "boolean", nullable: false),
                    DataHoraAceite = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Respostas = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_anamneses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_anamneses_pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "midias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PacienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedimentoId = table.Column<Guid>(type: "uuid", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChaveArmazenamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NomeArquivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    Etiqueta = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsentimentoClinico = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentimentoPublico = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_midias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_midias_pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_midias_procedimentos_ProcedimentoId",
                        column: x => x.ProcedimentoId,
                        principalTable: "procedimentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "agendamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PacienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioProfissionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedimentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    SolicitacaoAtendimentoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ValorCobrado = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MotivoCancelamento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agendamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agendamentos_pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_agendamentos_procedimentos_ProcedimentoId",
                        column: x => x.ProcedimentoId,
                        principalTable: "procedimentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_agendamentos_usuarios_UsuarioProfissionalId",
                        column: x => x.UsuarioProfissionalId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitacoes_atendimento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PacienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnamneseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedimentoInteresseId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcedimentoInteresseTexto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DataEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MotivoRecusa = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UsuarioTriagemId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataDecisao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AgendamentoId = table.Column<Guid>(type: "uuid", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitacoes_atendimento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_solicitacoes_atendimento_agendamentos_AgendamentoId",
                        column: x => x.AgendamentoId,
                        principalTable: "agendamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_solicitacoes_atendimento_anamneses_AnamneseId",
                        column: x => x.AnamneseId,
                        principalTable: "anamneses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_solicitacoes_atendimento_pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_solicitacoes_atendimento_procedimentos_ProcedimentoInteress~",
                        column: x => x.ProcedimentoInteresseId,
                        principalTable: "procedimentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_solicitacoes_atendimento_usuarios_UsuarioTriagemId",
                        column: x => x.UsuarioTriagemId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agendamentos_PacienteId",
                table: "agendamentos",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_agendamentos_ProcedimentoId",
                table: "agendamentos",
                column: "ProcedimentoId");

            migrationBuilder.CreateIndex(
                name: "IX_agendamentos_UsuarioProfissionalId_Inicio_Fim",
                table: "agendamentos",
                columns: new[] { "UsuarioProfissionalId", "Inicio", "Fim" });

            migrationBuilder.CreateIndex(
                name: "IX_anamnese_links_PacienteId",
                table: "anamnese_links",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_anamnese_links_Token",
                table: "anamnese_links",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_anamneses_PacienteId_DataPreenchimento",
                table: "anamneses",
                columns: new[] { "PacienteId", "DataPreenchimento" });

            migrationBuilder.CreateIndex(
                name: "IX_logs_auditoria_DataHora",
                table: "logs_auditoria",
                column: "DataHora");

            migrationBuilder.CreateIndex(
                name: "IX_logs_auditoria_EntidadeTipo_EntidadeId",
                table: "logs_auditoria",
                columns: new[] { "EntidadeTipo", "EntidadeId" });

            migrationBuilder.CreateIndex(
                name: "IX_midias_PacienteId_Etiqueta",
                table: "midias",
                columns: new[] { "PacienteId", "Etiqueta" });

            migrationBuilder.CreateIndex(
                name: "IX_midias_ProcedimentoId",
                table: "midias",
                column: "ProcedimentoId");

            migrationBuilder.CreateIndex(
                name: "IX_pacientes_ClinicaId_Cpf",
                table: "pacientes",
                columns: new[] { "ClinicaId", "Cpf" },
                unique: true,
                filter: "\"Cpf\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_pacientes_ClinicaId_Telefone",
                table: "pacientes",
                columns: new[] { "ClinicaId", "Telefone" });

            migrationBuilder.CreateIndex(
                name: "IX_procedimentos_ClinicaId_Nome",
                table: "procedimentos",
                columns: new[] { "ClinicaId", "Nome" });

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_atendimento_AgendamentoId",
                table: "solicitacoes_atendimento",
                column: "AgendamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_atendimento_AnamneseId",
                table: "solicitacoes_atendimento",
                column: "AnamneseId");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_atendimento_ClinicaId_Status",
                table: "solicitacoes_atendimento",
                columns: new[] { "ClinicaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_atendimento_PacienteId",
                table: "solicitacoes_atendimento",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_atendimento_ProcedimentoInteresseId",
                table: "solicitacoes_atendimento",
                column: "ProcedimentoInteresseId");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_atendimento_UsuarioTriagemId",
                table: "solicitacoes_atendimento",
                column: "UsuarioTriagemId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_ClinicaId",
                table: "usuarios",
                column: "ClinicaId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anamnese_links");

            migrationBuilder.DropTable(
                name: "logs_auditoria");

            migrationBuilder.DropTable(
                name: "midias");

            migrationBuilder.DropTable(
                name: "solicitacoes_atendimento");

            migrationBuilder.DropTable(
                name: "agendamentos");

            migrationBuilder.DropTable(
                name: "anamneses");

            migrationBuilder.DropTable(
                name: "procedimentos");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "pacientes");

            migrationBuilder.DropTable(
                name: "Clinicas");
        }
    }
}
