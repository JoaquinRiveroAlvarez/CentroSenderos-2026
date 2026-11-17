# Áreas en Administración de prestaciones

En Administración → Tipo Prestación, Crear y Editar incorporan el selector de áreas activas del catálogo Tipo Área. El formulario exige seleccionar un área activa antes de guardar. Listado y detalle muestran el nombre del área, o «Sin área asignada» para registros existentes sin clasificar. La búsqueda incluye el nombre del área.

La API devuelve `TipoAreaId` y `TipoAreaNombre` en las consultas por identificador, código y listado. Si se envía un área, el servidor comprueba que exista y esté activa. El nombre mostrado se consulta mediante la relación; no se guarda una copia ni se confía en el nombre enviado por el cliente.

Los contratos anteriores siguen siendo compatibles mientras se adaptan el alta rápida y el resto del sistema: el área continúa opcional en el DTO y en la BD. Un alta anterior sin área se permite y queda pendiente de clasificación. Una edición que omite `TipoAreaId` conserva la asignación existente; no permite borrarla accidentalmente. Esto es una transición: la obligatoriedad del formulario de Administración todavía no es una obligación global de todas las API.

Si no hay áreas activas, se informa dónde crearlas y se bloquea Guardar. Si falla la carga, se ofrece reintentar. Un área asignada que ya no esté disponible se sigue mostrando al editar, pero debe elegirse una activa para guardar desde Administración.

## Base de datos

Se necesita la migración `20261117170152_RelacionOpcionalPrestacionArea` de la entrega anterior. Esta entrega no agrega otra migración ni aplica cambios a Neon o Render. El modelo nuevo debe usarse con esa columna ya creada.

## Verificación

- Abrir un registro sin área y comprobar la etiqueta y el detalle.
- Crear una prestación con un área y verificar que aparezca en el listado.
- Cambiar el área desde Editar y volver a abrir para comprobar su persistencia.
- Buscar por el nombre del área.
- Comprobar carga fallida, reintento, catálogo vacío y área no disponible.
- Probar altas y ediciones anteriores sin área para confirmar su compatibilidad.

Pruebas automatizadas aisladas: asignación y cambio de área, lectura por ID/código/listado, renombrado del área, rechazo de IDs inválidos o áreas inactivas/borradas, conservación del vínculo en clientes anteriores y validación de retención. Se verifican junto con las pruebas de relación opcional y de profesionales/matrículas. La UI se comprueba con datos ficticios en un servidor local.
