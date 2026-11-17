# Sesión individual

«Sesión normal» pasa a llamarse «Sesión individual» y requiere exactamente un paciente. La regla se aplica en Crear, Editar, duplicación y series recurrentes. El servidor valida la cantidad antes de modificar o guardar turnos, incluso si el tipo fue configurado por error con pacientes opcionales.

En Crear y Editar, elegir otro paciente reemplaza el seleccionado. Los demás tipos de turno conservan su selección múltiple. Si se abre o duplica un turno viejo con varios pacientes, o se cambia de otro tipo a individual teniendo varios seleccionados, se muestran todos y un aviso: hay que elegir uno o quitar los demás antes de guardar. No se eliminan pacientes de turnos históricos automáticamente.

Tipo de turno y Consultorio usan la misma familia, tamaño (14 px), peso y altura de línea que los otros controles de los formularios; sus opciones y búsquedas usan 13 px.

## Actualización

Aplicar la migración `20261117120000_RenombrarSesionIndividual` a la base usada por la aplicación. En la consola del Administrador de paquetes de Visual Studio, con el proyecto Server como inicio:

```powershell
Update-Database -Project CentroSenderos-2026-BD -StartupProject CentroSenderos-2026-Server
```

La migración renombra el registro manteniendo su identificador, duración y relaciones con todos los turnos existentes. También activa la obligatoriedad del paciente para ese tipo. No agrega columnas ni modifica los pacientes asociados. La compatibilidad con el nombre anterior mantiene la validación durante la actualización.

Si ya existen simultáneamente ambos nombres en el catálogo, hay que revisar esos registros antes de migrar: el índice de nombres únicos impedirá renombrar el anterior a un nombre que ya existe. No se fusionan tipos ni se borran relaciones automáticamente.

## Verificación

Pruebas aisladas con SQLite en memoria: cantidades cero/uno/varios, nombre anterior y nuevo, configuración opcional, altas y ediciones, persistencia de un paciente en turnos simples y series, rechazo sin alterar datos guardados, normalización del catálogo y migración de datos. La prueba de migración usa equivalentes locales de `btrim` y `translate`; no requiere Neon.

En navegador con datos ficticios: selección y reemplazo en Crear/Editar, aviso de turnos viejos con varios pacientes, selección múltiple de otros tipos y comparación de tipografía calculada de los controles.
