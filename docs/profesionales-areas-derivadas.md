# Áreas de profesionales obtenidas desde sus prestaciones

Crear y Editar profesional muestran áreas de solo lectura a partir de las prestaciones seleccionadas. No se elige un área independiente. Una prestación aporta su área; varias prestaciones de la misma área no repiten la etiqueta. Al agregar o quitar una prestación se recalcula la presentación y se conserva la matrícula de las restantes.

El listado, su búsqueda, el ordenamiento, el filtro múltiple de áreas y el detalle usan la colección `Areas` enviada por el servidor. El filtro incluye al profesional cuando cualquiera de sus áreas coincide con una opción marcada. Los parámetros de retorno al listado se conservan como antes.

El alta rápida de prestaciones incluye selección obligatoria de área activa y un botón para crear un área sin salir del formulario. Se conserva lo escrito en la prestación y en el profesional. Un área recién creada se selecciona en la prestación; al guardarla se agrega al profesional y se solicita su matrícula. Las altas rápidas de obra social, diagnóstico y documentos no cambian.

## Compatibilidad con prestaciones pendientes

Las prestaciones anteriores sin área no se clasifican automáticamente. Crear/Editar profesional muestran un aviso para completarlas desde Administración → Tipo Prestación. Se conserva la posibilidad de editar esos profesionales y sus matrículas; no se bloquea toda la carga por datos que todavía están pendientes de clasificar.

La colección `Areas` contiene exclusivamente áreas de las prestaciones vinculadas; el servidor ignora un área manual enviada por el cliente. Las consultas por ID, CUIT y listado calculan la colección en base a la relación, por lo que renombrar un área o reasignar una prestación se refleja sin volver a editar los profesionales.

Se mantiene temporalmente `Profesionales.Area` en la BD. Al guardar se conserva allí una primera área derivada para compatibilidad; si las prestaciones siguen sin clasificar se preserva el dato anterior. El DTO conserva `Area` como resumen de las áreas calculadas para clientes anteriores; las pantallas actuales de Turnos y Socios ya usan la colección `Areas`; si aún no hay áreas calculadas, conserva el texto legado. Ese texto no se usa para las etiquetas ni para el filtro nuevo de Profesionales. La retirada definitiva del campo pertenece a una entrega posterior.

## Verificación

- Crear con una prestación: una etiqueta de área.
- Agregar otra prestación de la misma área: una sola etiqueta, con dos matrículas independientes.
- Agregar una prestación de otra área: dos etiquetas.
- Quitar una prestación: recalcular áreas y conservar las matrículas restantes.
- Crear prestación y área desde el alta rápida: conservar ambos formularios y seleccionar el resultado.
- Ver y editar un profesional existente, incluyendo prestaciones pendientes de clasificación.
- Filtrar por cualquiera de las áreas, combinar varias opciones y limpiar filtros.
- Probar nombres de áreas actualizados y cambios de área de una prestación.

Se verifican consultas y persistencia en SQLite en memoria, más los flujos de Crear/Editar, filtros y alta rápida anidada en navegador con datos ficticios. No se consultó ni modificó Neon.

No agrega migraciones: requiere la relación opcional creada por `20261117170152_RelacionOpcionalPrestacionArea`, ya utilizada por Administración.
