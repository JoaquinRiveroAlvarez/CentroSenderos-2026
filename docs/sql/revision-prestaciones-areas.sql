-- Etapa 1. PostgreSQL. Solo lectura; funciona antes de agregar TipoAreaId.
-- No asigna áreas ni modifica profesionales, prestaciones o matrículas.
BEGIN TRANSACTION READ ONLY;

WITH vinculos AS (
    SELECT DISTINCT relacion."TipoPrestacionId", profesional."Id" AS profesional_id,
        NULLIF(lower(btrim(profesional."Area")), '') AS area_legada,
        area."Id" AS area_catalogo_id,
        area."Tipo" AS area_catalogo
    FROM "ProfesionalTipoPrestaciones" relacion
    JOIN "Profesionales" profesional ON profesional."Id" = relacion."ProfesionalId"
    LEFT JOIN "TipoAreas" area
        ON lower(btrim(area."Tipo")) = lower(btrim(profesional."Area"))
        AND area."EstadoRegistro" = 1
    WHERE relacion."EstadoRegistro" = 1
        AND profesional."EstadoRegistro" = 1
), resumen AS (
    SELECT prestacion."Id", prestacion."Tipo", prestacion."Cod", prestacion."EstadoRegistro",
        count(DISTINCT vinculo.profesional_id) AS profesionales_activos,
        count(DISTINCT vinculo.area_legada) AS areas_legadas_distintas,
        count(DISTINCT vinculo.area_catalogo_id) AS areas_catalogo_distintas,
        string_agg(DISTINCT vinculo.area_legada, ', ' ORDER BY vinculo.area_legada) AS areas_legadas,
        string_agg(DISTINCT vinculo.area_catalogo, ', ' ORDER BY vinculo.area_catalogo) AS candidatas_catalogo,
        bool_or(vinculo.profesional_id IS NOT NULL AND vinculo.area_legada IS NULL) AS tiene_area_vacia,
        bool_or(vinculo.profesional_id IS NOT NULL AND vinculo.area_catalogo_id IS NULL) AS tiene_area_sin_catalogo
    FROM "TipoPrestaciones" prestacion
    LEFT JOIN vinculos vinculo ON vinculo."TipoPrestacionId" = prestacion."Id"
    GROUP BY prestacion."Id", prestacion."Tipo", prestacion."Cod", prestacion."EstadoRegistro"
)
SELECT *, CASE
    WHEN profesionales_activos = 0 THEN 'SIN_PROFESIONALES_ACTIVOS'
    WHEN areas_legadas_distintas > 1 THEN 'MULTIPLES_AREAS_LEGADAS'
    WHEN tiene_area_vacia THEN 'AREA_LEGADA_VACIA'
    WHEN tiene_area_sin_catalogo THEN 'AREA_NO_EN_CATALOGO_ACTIVO'
    WHEN areas_catalogo_distintas > 1 THEN 'CATALOGO_AMBIGUO'
    WHEN areas_catalogo_distintas = 1 THEN 'CANDIDATA_UNICA_PARA_REVISAR'
    ELSE 'REVISAR_MANUALMENTE'
END AS revision
FROM resumen
ORDER BY revision, "Tipo", "Id";

ROLLBACK;
