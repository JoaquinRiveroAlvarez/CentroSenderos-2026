# Buscadores y filtros

## Alcance

Los desplegables de crear y editar Pacientes, Profesionales y Turnos incorporan búsqueda individual. Incluye los tipos de documento, obras sociales y diagnósticos, la incorporación de prestaciones al profesional, roles, prestaciones de cada atención y las opciones de recurrencia. El área se busca también al crear una prestación desde el modal con + y desde Administración.

Los campos de formulario siguen siendo de selección simple. Seleccionar varias prestaciones para un profesional continúa agregándolas una por una, con su matrícula. La entrada reutilizable conserva el valor tipado y la validación del formulario, admite quitar la selección y respeta el estado deshabilitado. Las opciones de ordenamiento mantienen siempre una selección.

## Filtros de listados

- Pacientes: obra social, diagnóstico, CUD, rango de edad, con/sin documentos y tipos de documentos cargados.
- Profesionales: área, prestación, socio y rol.
- Turnos: consultorio, tipo de turno, profesional, paciente, área, prestación y estado. Cada grupo tiene su propia búsqueda.
- Administración: búsqueda por nombre y descripción; también código en Prestaciones, CUIT en Obras Sociales, dirección en Consultorios y duración en Tipos de Turno. Prestaciones incorpora además selección múltiple de áreas.

Dentro de un filtro se combinan opciones con «o»; entre filtros, con «y». Sin opciones seleccionadas no se restringe ese grupo. En Turnos, área y prestación deben coincidir en una misma atención: un turno grupal no aparece por combinar el área de un profesional con la prestación de otro.

Las búsquedas de desplegables y catálogos ignoran mayúsculas y tildes. «Limpiar filtros» elimina las selecciones y búsquedas correspondientes. Los nuevos filtros de entidades se conservan en el contexto de navegación al abrir y volver desde un registro.

Los rangos de edad son 0–5, 6–12, 13–17, 18–59 y 60 años o más. Hay una opción para pacientes sin fecha de nacimiento. La edad respeta el cumpleaños. Los filtros de documentos consideran solamente documentos activos y consultan sus tipos; no descargan contenido de archivos.

## Base de datos

No requiere migraciones. Se agregan metadatos a las respuestas de los listados: tipos de documentos del paciente y área de la prestación efectivamente asignada al turno. Se conservan las propiedades anteriores para compatibilidad.

## Comprobaciones

1. En crear y editar, abrir cada desplegable, buscar sin tildes, seleccionar, quitar y volver a seleccionar. Verificar las validaciones obligatorias y los botones +.
2. En Pacientes, combinar edad y documento; probar un paciente sin documentos y otro sin fecha de nacimiento.
3. En Profesionales, combinar área, prestación y rol; seleccionar más de una opción del mismo grupo y limpiar.
4. En Turnos, combinar área, prestación y estado en las vistas diaria y semanal. Verificar que «Limpiar filtros» destilda y que «Seleccionar todos» restaura los grupos.
5. Abrir un registro desde un listado filtrado y volver. Comprobar que se conservan los nuevos filtros.
6. En cada tabla tipo, buscar por nombre o descripción sin tildes y limpiar. En Prestaciones, combinar varias áreas con búsqueda por código y comprobar el área en crear/editar.
7. Repetir la selección y limpieza en móvil. Los menús tienen altura limitada y desplazamiento interno.

Las pruebas unitarias nuevas cubren los límites de edad y cumpleaños, los documentos activos mediante SQLite, la combinación de filtros de pacientes, la correspondencia de área/prestación del turno y la búsqueda sin tildes. Se mantienen las pruebas de regresión de documentos, áreas y sesión individual.
