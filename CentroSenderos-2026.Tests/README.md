# Pruebas de autenticación de Pacientes y Profesionales

Este proyecto reúne 19 casos relacionados con la corrección de SEG-001 y SEG-002. El resto de la suite de Sprint 1 permanece en la rama testing del fork.

## Alcance

- 10 operaciones de API sin sesión: listar, consultar, crear, editar y eliminar en ambos módulos.
- 2 consultas anónimas de registros inexistentes.
- 2 circuitos completos por interfaz con sesión, uno de pacientes y otro de profesionales.
- 4 comprobaciones de selectores al crear y editar Turnos y Socios.
- 1 comprobación de conectividad a la base de testing.

Las pruebas de seguridad deben rechazar solicitudes anónimas con 401, 403 o redirección al login. Los circuitos autenticados verifican persistencia y baja lógica.

## Entorno

Se requiere .NET 10, Visual Studio o el CLI de .NET, Chromium para Playwright y el Server local HTTPS en https://localhost:7069. CENTROSENDEROS_TEST_URL permite otra URL HTTPS de loopback. El proyecto usa xunit.v3 4.0.2 y Microsoft.Playwright 1.63.0.

Configurar en los secretos locales del proyecto Tests:

- ConnectionStrings:TestingConnection: conexión exclusiva de Neon testing.
- Frontend:Email y Frontend:Password: usuario existente en esa base.

Configurar ConnectionStrings:DefaultConnection en los secretos locales del Server con el mismo destino de testing. Reiniciar Server con el perfil https. Los circuitos comparan los destinos configurados, pero ese control no inspecciona las variables de un proceso que ya estaba abierto. No reutilizar un servidor conectado a otra base.

BaseDePruebas y ConexionTests conservan el host autorizado en la ejecución original. Para otro entorno de pruebas, revisar ambas guardas y verificar explícitamente la rama de Neon antes de ejecutar escrituras. Nunca quitar las guardas para usar una base de producción.

No guardar contraseñas, cookies ni cadenas de conexión en archivos versionados.

## Ejecución

1. Restaurar y compilar la solución.
2. Instalar Chromium con el script generado:
   powershell -ExecutionPolicy Bypass -File ".\\CentroSenderos-2026.Tests\\bin\\Debug\\net10.0\\playwright.ps1" install chromium
3. Iniciar el Server con Ctrl + F5, perfil https, y mantenerlo abierto.
4. Ejecutar las clases de este proyecto desde el Explorador de pruebas. Las pruebas están configuradas en serie porque comparten servidor y base.

Los circuitos completos y las pruebas HTTP anónimas crean datos ficticios. El bloque finally intenta dar de baja lógicamente solo sus propios registros ante un fallo. La baja lógica conserva filas, y no se certificó limpieza independiente de residuos. Las regresiones de Turnos y Socios requieren registros existentes y no guardan cambios.

## Validación y límites

Los mismos archivos de aplicación y pruebas se ejecutaron localmente en la rama testing: las operaciones sin sesión quedaron bloqueadas y los circuitos con sesión y las regresiones de selectores resultaron aprobados. En una ejecución conjunta hubo timeouts de login y Neon; los casos afectados pasaron en repeticiones individuales.

La selección aislada de este pull request requiere restauración y ejecución propia antes de integrar. El entorno de preparación no dispone de dotnet; no se declara compilación o ejecución de esta rama aislada.

No se modificaron roles, migraciones ni reglas de negocio. FUN-001 (prestaciones ausentes del listado), la falta de DELETE de consultorios y la posible colisión concurrente de afiliados siguen fuera de esta corrección. Tampoco se certificaron expiración/revocación de sesión, dos usuarios simultáneos o el despliegue público. La corrección del servidor solo reenvía la cookie al mismo origen; requiere ApiBaseUrl consistente con el origen utilizado por la aplicación.
