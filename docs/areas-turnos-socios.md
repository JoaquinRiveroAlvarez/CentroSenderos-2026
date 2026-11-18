# Áreas derivadas en Turnos y Socios

Crear y Editar turno muestran las áreas obtenidas desde las prestaciones del profesional en las opciones de selección, las tarjetas seleccionadas y el resumen. La búsqueda de profesionales admite nombre, área y prestación, ignorando mayúsculas y tildes.

Socios muestra las áreas en las opciones del selector y en etiquetas debajo del profesional elegido, tanto en las pantallas independientes como en los formularios del listado. Listado y detalle también muestran las áreas; la búsqueda del listado incluye las áreas. Caja Senderos conserva su presentación sin profesional ni áreas.

Las pantallas utilizan `ProfesionalListadoDTO.Areas`. No leen el texto legado `Area`, y una colección vacía se muestra como «Sin área asignada». No se cambia cómo se eligen profesionales o prestaciones, ni las reglas de disponibilidad de horarios, asistencia, pacientes o liquidaciones.

La consulta `docs/sql/estado-prestaciones-areas.sql` permite revisar el estado real de la clasificación después de aplicar la relación opcional. A diferencia del informe inicial, no propone áreas a partir del texto anterior del profesional. Debe usarse este estado actual antes de hacer obligatoria la relación y retirar la columna vieja.

Verificación: compilación, pruebas de resumen y búsqueda sin depender del campo anterior, regresión de áreas/matrículas y comprobación local de Crear/Editar turno y Socios con datos ficticios, incluyendo Caja Senderos. No requiere otra migración ni se aplica ningún cambio a Neon.
