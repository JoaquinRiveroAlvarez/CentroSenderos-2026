# Separación del catálogo por área

Se completó la clasificación de las prestaciones existentes usando el área histórica de cada profesional. Cuando una prestación se utilizaba en varias áreas, se creó una variante por área, incluyendo los vínculos inactivos y las referencias de turnos y liquidaciones.

Las variantes tienen nombres como `Psicología — Infantil` y códigos como `PSI-A1`. Se conserva el identificador original para la primera área, ordenada por identificador; las otras variantes reciben identificadores nuevos. Las prestaciones que ya tenían área permanecen sin cambios.

El procedimiento está en [separar-prestaciones-por-area.sql](sql/separar-prestaciones-por-area.sql). Debe ejecutarse dentro de una transacción y con un respaldo previo. Bloquea las escrituras concurrentes sobre las tablas involucradas mientras clasifica y reasigna las referencias. Si encuentra una prestación sin evidencia histórica o un área ambigua, cancela la operación. No debe utilizarse para adivinar áreas de prestaciones nuevas.

Se conservan aranceles, retenciones, descripciones, observaciones, estados, matrículas y todos los demás datos de las relaciones. Los detalles de liquidaciones se reasignan por el profesional de su liquidación, sin recalcular importes. Socios conserva su vínculo con el profesional y obtiene sus áreas a través de las prestaciones, como ya implementa la aplicación.

## Aplicación del 17 de noviembre de 2026

Se aplicó a la conexión actual `neondb`, con respaldo privado fuera del repositorio y una comprobación de que el estado no había cambiado entre el respaldo y la transacción.

- Catálogo: pasó de 17 a 30 prestaciones; ninguna quedó sin área.
- Profesionales: se conservaron los 32 vínculos; 15 referencias cambiaron de variante.
- Turnos: se conservaron las 69 asignaciones; 17 referencias cambiaron de variante.
- Socios: los dos socios activos con profesional tienen áreas derivadas.
- Liquidaciones: no había detalles cargados; las cabeceras existentes permanecieron sin cambios.

Se ensayó primero sobre copias temporales, con los índices únicos originales. Después de aplicar, se compararon los datos con el respaldo y se verificó la correspondencia de cada referencia con el área de su profesional. Una segunda ejecución sobre copias temporales confirmó que no duplica variantes.

No se eliminó la columna histórica `Profesionales.Area` ni se hizo obligatoria la FK en el esquema. Ese cierre requiere una migración posterior; no es necesario para ver las áreas derivadas en las pantallas actuales.

## Comprobación desde la aplicación

1. En Administración → Prestaciones, buscar una prestación separada y comprobar su área, arancel y retención.
2. En Profesionales, abrir un profesional con varias prestaciones y comprobar las áreas y matrículas.
3. Abrir un turno existente y verificar profesional y prestación; probar también crear uno nuevo.
4. En Socios, comprobar las etiquetas de área y el selector de profesional. Caja Senderos sigue sin profesional asociado.

La actualización de datos ya está aplicada en Neon. El script se guarda para revisión y otras bases; no hace falta volver a ejecutarlo sobre esta conexión.
