# Cambios de profesionales, pacientes y turnos

- La hora tiene un recuadro gris azulado con ícono de reloj en las vistas de lista y grilla. Los estados mantienen su paleta azul, verde, gris y naranja.
- El POST de profesional crea su cuenta Identity con el email normalizado y contraseña inicial `123456`, hasheada. Si no se indica rol, asigna `profesional`. Solo administradores pueden asignar un rol diferente. Un email ya registrado se rechaza sin modificar esa cuenta. Cuenta, rol y profesional se guardan en una transacción.
- Cada relación profesional/prestación guarda su matrícula (obligatoria, hasta 30 caracteres). Las altas, ediciones, listados y fichas usan `MatriculasPorPrestacion`. `MP` se conserva como dato de compatibilidad y copia la primera matrícula al guardar; ya no impone unicidad global.
- Desde crear/editar profesional se pueden crear áreas y prestaciones. Desde crear/editar paciente se pueden crear obras sociales y diagnósticos. Los modales validan los datos, conservan el formulario principal y seleccionan el nuevo registro al guardar.
- `GET api/turno/Horarios` devuelve todos los inicios candidatos y sus conflictos, con consultorio, profesionales, pacientes y rango horario. Reutiliza los criterios de bloqueo existentes y excluye el turno editado y los cancelados. El endpoint `Disponibles` conserva su contrato. Los inicios ocupados se muestran en la misma grilla, en rojo suave; se pueden consultar sin seleccionarlos.

## Migración

Antes de ejecutar esta versión contra la base configurada:

```sh
dotnet ef database update --project CentroSenderos-2026-BD --startup-project CentroSenderos-2026-Server/CentroSenderos-2026-Server
```

`MatriculaPorPrestacion` agrega la columna y copia la matrícula anterior a todas las prestaciones existentes. No crea cuentas retroactivamente. Un rollback elimina las matrículas por prestación y requiere que los valores de `MP` vuelvan a ser únicos para restablecer el índice anterior.

## Verificación aislada

`CambiosTurnosTests` utiliza SQLite en memoria y no necesita Neon, un servidor ni credenciales. Comprueba contraseña/rol, rollback del alta, conservación de cuentas existentes, matrículas por prestación, actualización de relaciones y conflictos por consultorio/profesional/paciente, cancelación y exclusión al editar.

```sh
dotnet build CentroSenderos-2026.Tests/CentroSenderos-2026.Tests.csproj
dotnet CentroSenderos-2026.Tests/bin/Debug/net10.0/CentroSenderos-2026.Tests.dll -class CentroSenderos_2026.Tests.Unitarias.CambiosTurnosTests
```

Las pruebas HTTP, de navegador y de integración con Neon del repositorio necesitan su entorno de testing configurado; no están incluidas en esa ejecución aislada.

## Ajuste visual y filtros

Los cuatro modales de alta rápida usan la paleta oliva y fondos cálidos del sistema. Se abren desde el botón `+` junto a cada selector, mantienen el foco dentro del diálogo y permiten cerrar con Escape o Cancelar sin perder el formulario principal. Las prestaciones muestran su matrícula en una fila y al asignarlas a un turno.

La disponibilidad usa una grilla común de horarios libres, ocupados y seleccionados. Consultar uno ocupado abre el detalle debajo sin elegirlo ni cambiar el horario seleccionado. El detalle distingue conflictos de consultorio, profesional y paciente. El recuadro de la hora en lista, grilla y detalle utiliza un gris azulado con ícono de reloj.

En Turnos, **Limpiar filtros** destilda los cuatro grupos; con cero opciones seleccionadas se muestran todos los turnos, según la regla existente. **Seleccionar todos** vuelve a tildarlos. Los filtros vacíos se conservan al volver desde otra pantalla. Profesionales y Pacientes mantienen **Limpiar filtros**, que reinicia selectores y búsqueda.
