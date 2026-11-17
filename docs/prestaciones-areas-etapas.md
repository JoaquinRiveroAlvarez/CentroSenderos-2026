# Relación entre prestaciones y áreas: implementación por etapas

## Etapa 1: preparar y revisar

Esta entrega agrega `TipoPrestaciones.TipoAreaId` como clave foránea **opcional** hacia `TipoAreas`, con índice y eliminación restringida. Conserva `Profesionales.Area`, las matrículas por prestación y los contratos actuales de los formularios y las API. Las prestaciones existentes quedan con el nuevo campo vacío: no se asignan áreas por coincidencias de nombres ni por el primer profesional encontrado.

La migración es `20261117170152_RelacionOpcionalPrestacionArea`. Su `Up` solo agrega una columna nullable, un índice y una clave foránea. No cambia Identity ni actualiza registros. El catálogo de áreas también impide dar de baja un área vinculada a prestaciones no borradas; las restricciones anteriores de profesionales continúan vigentes.

### Alcance encontrado en el código

| Parte | Uso actual y trabajo pendiente |
|---|---|
| Entidad y DTO de Profesional | `Area` sigue siendo un texto obligatorio. Se conserva durante la transición. |
| `ProfesionalRepositorio` | Obtiene las áreas desde las prestaciones y conserva temporalmente el campo legado para compatibilidad. |
| `TipoAreaRepositorio` | Renombra el texto legado y bloquea bajas de áreas usadas; ahora también protege prestaciones vinculadas. |
| Crear, Editar, Detalle y Listado de Profesionales | Áreas automáticas en los formularios; listado, detalle, búsqueda y filtros usan la colección derivada. |
| Crear y Editar Turno | Muestran las áreas derivadas en opciones, tarjetas y resúmenes; la búsqueda admite áreas y prestaciones. |
| Crear, Editar y Listado de Socios | Usan las áreas derivadas del profesional asociado; Caja se presenta sin áreas. |
| Prestaciones y alta rápida | Administración ya permite seleccionar un área activa; el alta rápida también permite seleccionar o crear un área. |
| Reportes y liquidaciones | La búsqueda de referencias directas a `.Area` no encontró usos adicionales aquí. Se deberán comprobar los consumidores indirectos del DTO al cambiar su contrato. |

### Prueba sobre una copia de la base

1. Preparar una copia restaurada de la base, con respaldo y una configuración local que apunte a esa copia. Comprobar el host y el nombre antes de ejecutar herramientas de actualización. Esta entrega no aplica nada a Neon ni a Render.
2. Ejecutar `docs/sql/revision-prestaciones-areas.sql` en esa copia. Es una transacción de solo lectura y funciona con el esquema anterior. El informe incluye todas las prestaciones y toma como evidencia las relaciones y los profesionales activos.
3. Revisar los casos sin profesionales, con áreas vacías, con varias áreas legadas, sin coincidencia en el catálogo activo o con coincidencias ambiguas. Incluso una candidata única requiere revisión. La comparación solo normaliza espacios exteriores y mayúsculas; no fusiona nombres parecidos ni elimina tildes.
4. En la consola del Administrador de paquetes de Visual Studio, con el proyecto Server como inicio y la conexión ya dirigida a la copia:

   ```powershell
   Update-Database -Migration 20261117170152_RelacionOpcionalPrestacionArea -Project CentroSenderos-2026-BD -StartupProject CentroSenderos-2026-Server
   ```

   Este comando aplica también las migraciones anteriores que le falten a la copia. Conviene revisar previamente su historial.
5. Confirmar que el nuevo campo está vacío en las prestaciones existentes y que los conteos de profesionales, prestaciones, matrículas y turnos no cambiaron. Probar altas y ediciones de prestaciones y profesionales, turnos y socios con el funcionamiento anterior.
6. Conservar el informe revisado para preparar la asignación de áreas en la siguiente entrega. No ejecutar asignaciones masivas todavía.

La aplicación compilada con el modelo nuevo necesita esa columna antes de guardar entidades: al desplegar, primero se amplía el esquema y después se publica la aplicación. La versión anterior puede convivir con esta columna opcional. Para volver atrás se puede restaurar la aplicación anterior y conservar la ampliación; no retirar la columna si ya se cargaron asignaciones sin antes preservarlas.

### Verificaciones automatizadas

Pruebas con SQLite en memoria para la relación opcional, las altas y ediciones anteriores, conservación de una asignación al editar desde el contrato anterior, múltiples prestaciones por área, integridad de la clave foránea y bajas protegidas. Además se genera el SQL PostgreSQL sin abrir conexión: el modelo coincide con el snapshot y la migración tiene exactamente las tres operaciones previstas. Las pruebas existentes de profesionales y matrículas sirven de regresión.

## Etapas siguientes

2. Administración ya incluye selección de área al crear y editar prestaciones, y muestra las que faltan clasificar. Completar las asignaciones desde ahí. El alta rápida ya permite seleccionar o crear el área. Resolver los conflictos, incluyendo prestaciones inactivas o borradas antes de exigir `NOT NULL`.
3. Profesionales ya obtiene áreas desde las prestaciones en formularios, listado, detalle y filtros; el DTO mantiene un resumen compatible para Turnos y Socios. Se conserva el dato legado para prestaciones sin clasificar. Ver `profesionales-areas-derivadas.md`. Turnos y Socios ya muestran y buscan sobre la colección de áreas; no queda uso directo de `.Area` en las páginas. Ver `areas-turnos-socios.md`. Falta completar la clasificación y revisar los consumidores externos antes de retirar ese campo.
4. Cuando todas las prestaciones tengan área y los flujos estén verificados, hacer obligatorio `TipoAreaId` y retirar la selección independiente del profesional.
5. Eliminar `Profesionales.Area` en una entrega posterior, después de comprobar que ya no tenga consumidores y de preservar el dato legado.

Cada etapa tiene su revisión y validación antes de continuar con la siguiente. Esta rama contiene la relación opcional, la selección de área en Administración y su uso derivado en Profesionales. Ver `administracion-prestaciones-areas.md` para el alcance de los formularios.
