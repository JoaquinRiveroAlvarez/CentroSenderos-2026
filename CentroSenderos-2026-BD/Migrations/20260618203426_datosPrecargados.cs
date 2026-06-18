using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class datosPrecargados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insertar Tipos de Prestación
            migrationBuilder.InsertData(
                table: "TipoPrestaciones",
                columns: new[] { "Tipo", "Descripcion", "Cod", "MontoSesion", "Observacion", "EstadoRegistro" },
                values: new object[,]
                {
                    { "Psicomotricidad", "Estimulación y desarrollo de habilidades psicomotoras", "PSI-001", 150.00m, "Sesión individual de 60 minutos", 0 },
                    { "Talleres", "Talleres terapéuticos grupales", "TAL-001", 100.00m, "Sesión grupal de 90 minutos", 0 },
                    { "Psicopedagogía Clínica", "Evaluación y tratamiento de dificultades de aprendizaje", "PPC-001", 160.00m, "Sesión individual de 60 minutos", 0 },
                    { "Integración Escolar", "Acompañamiento en proceso de integración educativa", "INT-001", 145.00m, "Sesión individual de 60 minutos", 0 },
                    { "Fonoaudiología", "Evaluación y tratamiento de trastornos del lenguaje", "FON-001", 155.00m, "Sesión individual de 45 minutos", 0 },
                    { "Kinesiología - Neurorehabilitación", "Rehabilitación neuromotora y recuperación funcional", "KIN-001", 160.00m, "Sesión individual de 60 minutos", 0 }
                });

            // Insertar Tipos de Obra Social
            migrationBuilder.InsertData(
                table: "TipoObrasSociales",
                columns: new[] { "Tipo", "Nombre", "Descripcion", "Observacion", "EstadoRegistro" },
                values: new object[,]
                {
                    { "OSDE", "OSDE", "Organización de Servicios Directos Especializados", "Cobertura privada", 0 },
                    { "IOMA", "IOMA", "Instituto de Obra Médico Asistencial", "Obra social provincial", 0 },
                    { "SURA", "SURA", "SURA Seguros de Salud", "Aseguradora de salud", 0 },
                    { "Swiss Medical", "Swiss Medical", "Swiss Medical Group", "Seguros de salud privado", 0 },
                    { "Medifé", "Medifé", "Medifé Entidad de Medicina Prepaga", "Medicina prepaga", 0 },
                    { "Galeno", "Galeno", "Galeno Seguros de Salud", "Medicina prepaga", 0 },
                    { "BMI", "BMI", "Bupa Médica Internacional", "Cobertura de salud privada", 0 },
                    { "Accord Salud", "Accord Salud", "Accord Salud", "Medicina prepaga", 0 },
                    { "Particular", "Particular", "Paciente particular sin obra social", "Pago directo", 0 }
                });

            // Insertar Tipos de Diagnóstico
            migrationBuilder.InsertData(
                table: "TipoDiagnosticos",
                columns: new[] { "Tipo", "Descripcion", "Observacion", "EstadoRegistro" },
                values: new object[,]
                {
                    { "Trastorno del Espectro Autista (TEA)", "Trastorno del neurodesarrollo", "Requiere psicomotricidad, talleres, fonoaudiología e integración", 0 },
                    { "Parálisis Cerebral", "Alteración del control motor y movimiento", "Requiere kinesiología neurorehabilitación y psicomotricidad", 0 },
                    { "Síndrome de Down", "Alteración cromosómica", "Requiere psicomotricidad, psicopedagogía e integración escolar", 0 },
                    { "Discapacidad Intelectual", "Limitación significativa en funcionamiento intelectual", "Requiere psicopedagogía clínica y talleres", 0 },
                    { "Dislexia", "Dificultad específica en la lectura y escritura", "Requiere psicopedagogía clínica", 0 },
                    { "Dispraxia", "Dificultad en la coordinación motora", "Requiere psicomotricidad y kinesiología", 0 },
                    { "Trastorno del Lenguaje", "Dificultades en la expresión o comprensión del lenguaje", "Requiere fonoaudiología y psicopedagogía", 0 },
                    { "Trastorno del Déficit de Atención e Hiperactividad (TDAH)", "Alteración en atención y control de impulsos", "Requiere psicomotricidad, talleres y psicopedagogía", 0 },
                    { "Trastorno Generalizado del Desarrollo (TGD)", "Alteración en el desarrollo global", "Requiere evaluación multiprofesional", 0 },
                    { "Hipoacusia", "Pérdida auditiva", "Requiere fonoaudiología", 0 }
                });

            // Insertar Profesionales con perfiles terapéuticos
            migrationBuilder.InsertData(
                table: "Profesionales",
                columns: new[] { "Nombre", "Area", "Cuit", "MP", "RNP", "Telefono", "Observacion", "EstadoRegistro" },
                values: new object[,]
                {
                    { "Lic. Juan Carlos López", "Psicomotricista", "20-12345678-9", "MP-001", "RNP-10001", "+54 11 2345-6789", "Especialista en psicomotricidad infantil", 0 },
                    { "Lic. María González Ruiz", "Psicóloga", "27-87654321-0", "MP-002", "RNP-10002", "+54 11 3456-7890", "Psicopedagoga clínica, coordina talleres", 0 },
                    { "Lic. Roberto Martínez Silva", "Psicopedagogo", "23-45678901-2", "MP-003", "RNP-10003", "+54 11 4567-8901", "Especialista en evaluación educativa", 0 },
                    { "Lic. Patricia Fernández López", "Psicopedagoga de Integración", "24-56789012-3", "MP-004", "RNP-10004", "+54 11 5678-9012", "Especialista en integración escolar", 0 },
                    { "Lic. Carlos Alberto Romero", "Fonoaudiólogo", "20-67890123-4", "MP-005", "RNP-10005", "+54 11 6789-0123", "Especialista en trastornos del lenguaje", 0 },
                    { "Lic. Ana María Torres García", "Kinesióloga", "25-78901234-5", "MP-006", "RNP-10006", "+54 11 7890-1234", "Especialista en neurorehabilitación", 0 },
                    { "Lic. Fernando Díaz Pérez", "Psicomotricista", "21-89012345-6", "MP-007", "RNP-10007", "+54 11 8901-2345", "Especialista en motricidad fina y gruesa", 0 },
                    { "Lic. Gabriela López Martínez", "Psicóloga", "22-90123456-7", "MP-008", "RNP-10008", "+54 11 9012-3456", "Psicopedagoga, facilita talleres grupales", 0 },
                    { "Lic. Miguel Ángel Suárez", "Fonoaudiólogo", "26-01234567-8", "MP-009", "RNP-10009", "+54 11 0123-4567", "Especialista en comunicación y deglución", 0 },
                    { "Lic. Verónica Castro Rodríguez", "Kinesióloga", "23-12345678-9", "MP-010", "RNP-10010", "+54 11 1234-5678", "Especialista en rehabilitación motora", 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar Profesionales
            migrationBuilder.DeleteData(
                table: "Profesionales",
                keyColumn: "Cuit",
                keyValue: "20-12345678-9");

            migrationBuilder.DeleteData(
                table: "Profesionales",
                keyColumn: "Cuit",
                keyValue: "27-87654321-0");

            migrationBuilder.DeleteData(
                table: "Profesionales",
                keyColumn: "Cuit",
                keyValue: "23-45678901-2");

            migrationBuilder.DeleteData(
                table: "Profesionales",
                keyColumn: "Cuit",
                keyValue: "24-56789012-3");

            migrationBuilder.DeleteData(
                table: "Profesionales",
                keyColumn: "Cuit",
                keyValue: "20-67890123-4");

            migrationBuilder.DeleteData(
                table: "Profesionales",
                keyColumn: "Cuit",
                keyValue: "25-78901234-5");

            migrationBuilder.DeleteData(
                table: "Profesionales",
                keyColumn: "Cuit",
                keyValue: "21-89012345-6");

            migrationBuilder.DeleteData(
                table: "Profesionales",
                keyColumn: "Cuit",
                keyValue: "22-90123456-7");

            migrationBuilder.DeleteData(
                table: "Profesionales",
                keyColumn: "Cuit",
                keyValue: "26-01234567-8");

            migrationBuilder.DeleteData(
                table: "Profesionales",
                keyColumn: "Cuit",
                keyValue: "23-12345678-9");

            // Eliminar Tipos de Prestación
            migrationBuilder.DeleteData(
                table: "TipoPrestaciones",
                keyColumn: "Tipo",
                keyValue: "Psicomotricidad");

            migrationBuilder.DeleteData(
                table: "TipoPrestaciones",
                keyColumn: "Tipo",
                keyValue: "Talleres");

            migrationBuilder.DeleteData(
                table: "TipoPrestaciones",
                keyColumn: "Tipo",
                keyValue: "Psicopedagogía Clínica");

            migrationBuilder.DeleteData(
                table: "TipoPrestaciones",
                keyColumn: "Tipo",
                keyValue: "Integración Escolar");

            migrationBuilder.DeleteData(
                table: "TipoPrestaciones",
                keyColumn: "Tipo",
                keyValue: "Fonoaudiología");

            migrationBuilder.DeleteData(
                table: "TipoPrestaciones",
                keyColumn: "Tipo",
                keyValue: "Kinesiología - Neurorehabilitación");

            // Eliminar Tipos de Obra Social
            migrationBuilder.DeleteData(
                table: "TipoObrasSociales",
                keyColumn: "Tipo",
                keyValue: "OSDE");

            migrationBuilder.DeleteData(
                table: "TipoObrasSociales",
                keyColumn: "Tipo",
                keyValue: "IOMA");

            migrationBuilder.DeleteData(
                table: "TipoObrasSociales",
                keyColumn: "Tipo",
                keyValue: "SURA");

            migrationBuilder.DeleteData(
                table: "TipoObrasSociales",
                keyColumn: "Tipo",
                keyValue: "Swiss Medical");

            migrationBuilder.DeleteData(
                table: "TipoObrasSociales",
                keyColumn: "Tipo",
                keyValue: "Medifé");

            migrationBuilder.DeleteData(
                table: "TipoObrasSociales",
                keyColumn: "Tipo",
                keyValue: "Galeno");

            migrationBuilder.DeleteData(
                table: "TipoObrasSociales",
                keyColumn: "Tipo",
                keyValue: "BMI");

            migrationBuilder.DeleteData(
                table: "TipoObrasSociales",
                keyColumn: "Tipo",
                keyValue: "Accord Salud");

            migrationBuilder.DeleteData(
                table: "TipoObrasSociales",
                keyColumn: "Tipo",
                keyValue: "Particular");

            // Eliminar Tipos de Diagnóstico
            migrationBuilder.DeleteData(
                table: "TipoDiagnosticos",
                keyColumn: "Tipo",
                keyValue: "Trastorno del Espectro Autista (TEA)");

            migrationBuilder.DeleteData(
                table: "TipoDiagnosticos",
                keyColumn: "Tipo",
                keyValue: "Parálisis Cerebral");

            migrationBuilder.DeleteData(
                table: "TipoDiagnosticos",
                keyColumn: "Tipo",
                keyValue: "Síndrome de Down");

            migrationBuilder.DeleteData(
                table: "TipoDiagnosticos",
                keyColumn: "Tipo",
                keyValue: "Discapacidad Intelectual");

            migrationBuilder.DeleteData(
                table: "TipoDiagnosticos",
                keyColumn: "Tipo",
                keyValue: "Dislexia");

            migrationBuilder.DeleteData(
                table: "TipoDiagnosticos",
                keyColumn: "Tipo",
                keyValue: "Dispraxia");

            migrationBuilder.DeleteData(
                table: "TipoDiagnosticos",
                keyColumn: "Tipo",
                keyValue: "Trastorno del Lenguaje");

            migrationBuilder.DeleteData(
                table: "TipoDiagnosticos",
                keyColumn: "Tipo",
                keyValue: "Trastorno del Déficit de Atención e Hiperactividad (TDAH)");

            migrationBuilder.DeleteData(
                table: "TipoDiagnosticos",
                keyColumn: "Tipo",
                keyValue: "Trastorno Generalizado del Desarrollo (TGD)");

            migrationBuilder.DeleteData(
                table: "TipoDiagnosticos",
                keyColumn: "Tipo",
                keyValue: "Hipoacusia");
        }
    }
}
