# Filtros con búsqueda

En los listados de Profesionales y Pacientes, cada filtro tiene un buscador propio y permite marcar varias opciones sin cerrar el desplegable. La búsqueda ignora mayúsculas y tildes, y oculta opciones sin quitar las selecciones anteriores.

Las opciones del mismo filtro se combinan como alternativas: Área Salud y Educación muestra profesionales de cualquiera de esas áreas. Los distintos filtros se cruzan: al agregar una prestación, solo quedan los profesionales de esas áreas que la ofrecen. Sin opciones marcadas, ese filtro no limita los resultados.

«Limpiar selección» vacía solo ese filtro. «Limpiar filtros» también borra la búsqueda general y las búsquedas de todos los desplegables. Al abrir una ficha y volver, se conservan filtros, búsqueda y orden. Los enlaces usan parámetros repetidos (por ejemplo, `area=Salud&area=Educación`) y siguen aceptando enlaces anteriores con una sola opción.

En Crear y Editar turno, Tipo de turno y Consultorio tienen lupita y búsqueda, con selección única. Cambiar una selección actualiza la consulta de horarios como antes. Los profesionales y pacientes del turno conservan su selección múltiple.

## Verificación

- Compilar la solución y los proyectos de pruebas.
- En ambos listados, marcar dos opciones del mismo filtro y cruzarlas con otro filtro; comprobar la unión dentro del grupo y la intersección entre grupos.
- Buscar con y sin tildes en cada desplegable; comprobar que las búsquedas sean independientes y que las selecciones ocultas sigan marcadas.
- Probar limpiar un grupo, limpiar todos los filtros, abrir una ficha y volver.
- En Crear y Editar turno, buscar y cambiar Tipo de turno y Consultorio; comprobar que solo quede una opción elegida.
- Probar Escape, flechas y Enter, cierre al hacer clic afuera y vista móvil.

No requiere migraciones ni actualización de la base de datos.
