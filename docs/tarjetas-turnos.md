# Tarjetas del listado de turnos

Las vistas diaria en lista y semanal comparten una tarjeta por turno, sin encabezados de columnas. El orden en escritorio es horario, consultorio, paciente, profesional con su prestación debajo, estado y botón de edición.

El fondo es crema suave, con bordes cálidos y detalles verdes de la interfaz. El horario conserva el gris azulado y los estados mantienen sus colores existentes. Cada profesional aparece junto a su propia prestación; se conservan todos en los turnos con varias atenciones. La recurrencia sigue visible debajo del horario.

El nombre del paciente es un botón accesible por teclado que abre el detalle. Hacer clic en la tarjeta abre el mismo detalle; el lápiz lleva a edición sin abrirlo. En pantallas pequeñas los bloques se reorganizan y el panel de filtros pasa arriba del listado.

Verificación: compilación de aplicación y pruebas; navegador local con datos ficticios en vistas diaria y semanal, varias prestaciones, nombres largos, recurrencia, abrir/cerrar detalle y tamaños de escritorio, tablet y celular. La prueba existente de orden horario se actualiza para comprobar inicio y fin en sus dos líneas.

No requiere migraciones ni cambios en la base de datos por este diseño. La migración de «Sesión individual» del cambio anterior sigue siendo independiente.
