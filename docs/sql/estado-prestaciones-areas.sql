-- PostgreSQL. Auditoría posterior a la migración de la relación opcional.
-- Solo lectura: no deduce áreas a partir del texto legado del profesional.
BEGIN TRANSACTION READ ONLY;

SELECT prestacion."Id", prestacion."Tipo", prestacion."Cod", prestacion."EstadoRegistro",
       prestacion."TipoAreaId", area."Tipo" AS area,
       CASE
           WHEN prestacion."TipoAreaId" IS NULL THEN 'SIN_AREA_ASIGNADA'
           WHEN area."EstadoRegistro" <> 1 THEN 'AREA_NO_ACTIVA'
           ELSE 'ASIGNADA'
       END AS estado_area
FROM "TipoPrestaciones" prestacion
LEFT JOIN "TipoAreas" area ON area."Id" = prestacion."TipoAreaId"
ORDER BY estado_area, prestacion."Tipo", prestacion."Id";

-- Revisar también prestaciones inactivas o borradas antes de hacer NOT NULL.
SELECT count(*) AS prestaciones_totales,
       count(*) FILTER (WHERE "TipoAreaId" IS NULL) AS prestaciones_sin_area
FROM "TipoPrestaciones";

ROLLBACK;
