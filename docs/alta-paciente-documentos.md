# Documentos opcionales al crear paciente

El formulario Crear paciente incluye una sección «Documentos y estudios» con la misma paleta del sistema. Se selecciona un tipo y se adjuntan uno o varios archivos; pueden agregarse más archivos con otro tipo o quitarse antes de guardar.

Tanto en Crear como en Editar paciente, el botón `+` junto al selector abre el modal «Crear tipo de documento». Al guardar, el nuevo tipo aparece seleccionado sin perder los datos del paciente ni los adjuntos. El botón también está disponible cuando todavía no hay tipos. Se rechazan nombres vacíos o duplicados.

Los archivos quedan pendientes en el formulario. Al pulsar «Guardar paciente», se guardan el paciente, sus teléfonos y todos los documentos en una transacción. Si falla un documento, se revierte el alta completa y el formulario conserva los archivos para corregir y reintentar. Si hay un problema de conexión y no se puede confirmar el resultado, se indica revisar el listado antes de reintentar.

Sin adjuntos se conserva el endpoint JSON habitual. Con adjuntos se utiliza `POST api/paciente/insertar-con-documentos`, protegido por autenticación, con `pacienteJson`, `tipoDocumentoIds` y `archivos` como campos multipart. Los documentos usan el repositorio y almacenamiento existentes y se consultan luego desde Editar paciente.

Límites compartidos entre cliente y servidor: 10 archivos, 20 MB por archivo y 30 MB en total. Los archivos vacíos se rechazan. La carga de tipos de documento no impide crear un paciente sin documentos si falla.

No requiere migraciones ni cambios de conexión.

Verificación: pruebas aisladas con SQLite de guardado con y sin documentos, persistencia del contenido y tipos, validación y rollback ante fallo del segundo archivo; verificación en navegador con la API real sobre una base temporal de selección múltiple, archivos de distintos tipos, quitar adjuntos, guardado multipart, creación sin documentos, corrección de DNI duplicado sin perder adjuntos, rechazo de acceso anónimo y presentación en escritorio/celular. No se utilizó Neon ni Render para estas pruebas.
