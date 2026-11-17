# Error al cargar profesionales en Render

La API devuelve una página HTML de error en lugar de los datos del listado. La página genérica no contiene la excepción original; por sí sola no confirma la causa.

Desde la incorporación de matrículas por prestación, el listado consulta `ProfesionalTipoPrestaciones.Matricula`. La aplicación no ejecuta `Database.Migrate` al iniciar. Si la base usada por Render no recibió la migración correspondiente, PostgreSQL rechazará esa consulta.

## Comprobaciones para quien administra la base o Render

1. Revisar la excepción de la API en los logs del servicio al abrir Profesionales. Un error `42703` que mencione `Matricula` confirma que falta esa columna.
2. En la base que usa ese servicio, comprobar el esquema con esta consulta de solo lectura:

```sql
SELECT column_name
FROM information_schema.columns
WHERE table_schema = 'public'
  AND table_name = 'ProfesionalTipoPrestaciones'
  AND column_name = 'Matricula';
```

3. Revisar si se registró `20261116224229_MatriculaPorPrestacion` en `__EFMigrationsHistory`.
4. Si falta aplicar la migración, usar el proyecto actualizado, configurar `DefaultConnection` hacia la misma base usada por Render y ejecutar desde un entorno con el SDK de .NET y EF Core:

```sh
dotnet ef database update --project CentroSenderos-2026-BD --startup-project CentroSenderos-2026-Server/CentroSenderos-2026-Server
```

La migración agrega la columna y conserva la matrícula anterior en las prestaciones existentes. Aplicarla a una base local o de testing diferente no actualiza la base de Render. No publicar cadenas de conexión en el repositorio ni en mensajes.

La corrección de `HttpServicio` evita que una respuesta HTML se muestre como mensaje en la interfaz. No modifica el esquema ni corrige por sí sola la causa de un error 500.
