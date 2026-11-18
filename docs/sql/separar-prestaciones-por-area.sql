-- Ejecutar dentro de una transacción, después de respaldar y comprobar el estado.
-- Clasifica únicamente prestaciones sin área usando el área histórica del profesional.
-- Incluye referencias inactivas, turnos y liquidaciones; no modifica importes.
DO $separacion$
DECLARE
    variante RECORD;
    nuevo_id integer;
BEGIN
    LOCK TABLE "TipoAreas", "TipoPrestaciones", "Profesionales",
        "ProfesionalTipoPrestaciones", "TurnoTipoPrestaciones",
        "DetalleLiquidaciones", "Liquidaciones", "Socios"
        IN SHARE ROW EXCLUSIVE MODE;

    CREATE TEMP TABLE prestaciones_antes ON COMMIT DROP AS
        SELECT * FROM "TipoPrestaciones";
    CREATE TEMP TABLE profesionales_prestaciones_antes ON COMMIT DROP AS
        SELECT * FROM "ProfesionalTipoPrestaciones";
    CREATE TEMP TABLE turnos_prestaciones_antes ON COMMIT DROP AS
        SELECT * FROM "TurnoTipoPrestaciones";
    CREATE TEMP TABLE detalles_antes ON COMMIT DROP AS
        SELECT * FROM "DetalleLiquidaciones";

    CREATE TEMP TABLE usos_prestacion ON COMMIT DROP AS
        SELECT "TipoPrestacionId" AS prestacion_id, "ProfesionalId" AS profesional_id
        FROM "ProfesionalTipoPrestaciones"
        UNION
        SELECT "TipoPrestacionId", "ProfesionalId" FROM "TurnoTipoPrestaciones"
        UNION
        SELECT d."TipoPrestacionId", l."ProfesionalId"
        FROM "DetalleLiquidaciones" d
        JOIN "Liquidaciones" l ON l."Id" = d."LiquidacionId"
        WHERE d."TipoPrestacionId" IS NOT NULL;

    CREATE TEMP TABLE clasificacion_prestacion ON COMMIT DROP AS
        SELECT u.prestacion_id, u.profesional_id, a."Id" AS area_id
        FROM usos_prestacion u
        JOIN "TipoPrestaciones" t ON t."Id" = u.prestacion_id
        LEFT JOIN "Profesionales" p ON p."Id" = u.profesional_id
        LEFT JOIN "TipoAreas" a
            ON lower(btrim(a."Tipo")) = lower(btrim(p."Area"))
        WHERE t."TipoAreaId" IS NULL;

    IF EXISTS (SELECT 1 FROM clasificacion_prestacion WHERE area_id IS NULL)
        OR EXISTS (
            SELECT prestacion_id, profesional_id FROM clasificacion_prestacion
            GROUP BY prestacion_id, profesional_id HAVING count(*) <> 1
        ) OR EXISTS (
            SELECT 1 FROM "TipoPrestaciones" t
            WHERE t."TipoAreaId" IS NULL AND NOT EXISTS (
                SELECT 1 FROM clasificacion_prestacion c WHERE c.prestacion_id = t."Id"
            )
        ) THEN
        RAISE EXCEPTION 'Hay prestaciones sin clasificación histórica inequívoca.';
    END IF;

    CREATE TEMP TABLE variantes_prestacion (
        prestacion_id integer NOT NULL,
        area_id integer NOT NULL,
        destino_id integer NOT NULL,
        PRIMARY KEY (prestacion_id, area_id)
    ) ON COMMIT DROP;

    FOR variante IN
        SELECT c.prestacion_id, c.area_id, a."Tipo" AS area_nombre,
            row_number() OVER (PARTITION BY c.prestacion_id ORDER BY c.area_id) AS posicion
        FROM (SELECT DISTINCT prestacion_id, area_id FROM clasificacion_prestacion) c
        JOIN "TipoAreas" a ON a."Id" = c.area_id
        ORDER BY c.prestacion_id, c.area_id
    LOOP
        IF variante.posicion = 1 THEN
            nuevo_id := variante.prestacion_id;
            UPDATE "TipoPrestaciones" t
            SET "TipoAreaId" = variante.area_id,
                "Tipo" = t."Tipo" || ' — ' || variante.area_nombre,
                "Cod" = t."Cod" || '-A' || variante.area_id
            WHERE t."Id" = variante.prestacion_id;
        ELSE
            INSERT INTO "TipoPrestaciones" (
                "Cod", "MontoSesion", "Observacion", "EstadoRegistro", "Tipo",
                "Descripcion", "PorcentajeRetencion", "TipoAreaId"
            )
            SELECT t."Cod" || '-A' || variante.area_id, t."MontoSesion",
                t."Observacion", t."EstadoRegistro", t."Tipo" || ' — ' || variante.area_nombre,
                t."Descripcion", t."PorcentajeRetencion", variante.area_id
            FROM prestaciones_antes t WHERE t."Id" = variante.prestacion_id
            RETURNING "Id" INTO nuevo_id;
        END IF;
        INSERT INTO variantes_prestacion VALUES (
            variante.prestacion_id, variante.area_id, nuevo_id
        );
    END LOOP;

    UPDATE "ProfesionalTipoPrestaciones" r
    SET "TipoPrestacionId" = v.destino_id
    FROM clasificacion_prestacion c
    JOIN variantes_prestacion v USING (prestacion_id, area_id)
    WHERE r."TipoPrestacionId" = c.prestacion_id AND r."ProfesionalId" = c.profesional_id;

    UPDATE "TurnoTipoPrestaciones" r
    SET "TipoPrestacionId" = v.destino_id
    FROM clasificacion_prestacion c
    JOIN variantes_prestacion v USING (prestacion_id, area_id)
    WHERE r."TipoPrestacionId" = c.prestacion_id AND r."ProfesionalId" = c.profesional_id;

    UPDATE "DetalleLiquidaciones" d
    SET "TipoPrestacionId" = v.destino_id
    FROM "Liquidaciones" l, clasificacion_prestacion c
    JOIN variantes_prestacion v USING (prestacion_id, area_id)
    WHERE d."LiquidacionId" = l."Id" AND l."ProfesionalId" = c.profesional_id
        AND d."TipoPrestacionId" = c.prestacion_id;

    IF EXISTS (
        SELECT 1 FROM profesionales_prestaciones_antes a
        JOIN clasificacion_prestacion c
            ON c.prestacion_id = a."TipoPrestacionId" AND c.profesional_id = a."ProfesionalId"
        JOIN variantes_prestacion v USING (prestacion_id, area_id)
        JOIN "ProfesionalTipoPrestaciones" r ON r."Id" = a."Id"
        WHERE r."TipoPrestacionId" <> v.destino_id
    ) OR EXISTS (
        SELECT 1 FROM turnos_prestaciones_antes a
        JOIN clasificacion_prestacion c
            ON c.prestacion_id = a."TipoPrestacionId" AND c.profesional_id = a."ProfesionalId"
        JOIN variantes_prestacion v USING (prestacion_id, area_id)
        JOIN "TurnoTipoPrestaciones" r ON r."Id" = a."Id"
        WHERE r."TipoPrestacionId" <> v.destino_id
    ) OR EXISTS (
        SELECT 1 FROM detalles_antes a
        JOIN "Liquidaciones" l ON l."Id" = a."LiquidacionId"
        JOIN clasificacion_prestacion c
            ON c.prestacion_id = a."TipoPrestacionId" AND c.profesional_id = l."ProfesionalId"
        JOIN variantes_prestacion v USING (prestacion_id, area_id)
        JOIN "DetalleLiquidaciones" r ON r."Id" = a."Id"
        WHERE r."TipoPrestacionId" <> v.destino_id
    ) THEN
        RAISE EXCEPTION 'Una referencia no coincide con el área de su profesional.';
    END IF;

    -- Comparación completa, salvo la FK que deliberadamente cambia.
    IF EXISTS (
        (SELECT to_jsonb(r) - 'TipoPrestacionId' FROM "ProfesionalTipoPrestaciones" r
         EXCEPT SELECT to_jsonb(r) - 'TipoPrestacionId' FROM profesionales_prestaciones_antes r)
        UNION ALL
        (SELECT to_jsonb(r) - 'TipoPrestacionId' FROM profesionales_prestaciones_antes r
         EXCEPT SELECT to_jsonb(r) - 'TipoPrestacionId' FROM "ProfesionalTipoPrestaciones" r)
        UNION ALL
        (SELECT to_jsonb(r) - 'TipoPrestacionId' FROM "TurnoTipoPrestaciones" r
         EXCEPT SELECT to_jsonb(r) - 'TipoPrestacionId' FROM turnos_prestaciones_antes r)
        UNION ALL
        (SELECT to_jsonb(r) - 'TipoPrestacionId' FROM turnos_prestaciones_antes r
         EXCEPT SELECT to_jsonb(r) - 'TipoPrestacionId' FROM "TurnoTipoPrestaciones" r)
        UNION ALL
        (SELECT to_jsonb(r) - 'TipoPrestacionId' FROM "DetalleLiquidaciones" r
         EXCEPT SELECT to_jsonb(r) - 'TipoPrestacionId' FROM detalles_antes r)
        UNION ALL
        (SELECT to_jsonb(r) - 'TipoPrestacionId' FROM detalles_antes r
         EXCEPT SELECT to_jsonb(r) - 'TipoPrestacionId' FROM "DetalleLiquidaciones" r)
    ) THEN
        RAISE EXCEPTION 'Se alteraron datos distintos de las referencias a prestaciones.';
    END IF;

    IF EXISTS (SELECT 1 FROM "TipoPrestaciones" WHERE "TipoAreaId" IS NULL)
        OR EXISTS (
            SELECT 1 FROM variantes_prestacion v
            JOIN "TipoPrestaciones" t ON t."Id" = v.destino_id
            JOIN prestaciones_antes a ON a."Id" = v.prestacion_id
            WHERE (to_jsonb(t) - ARRAY['Id','Tipo','Cod','TipoAreaId'])
                IS DISTINCT FROM (to_jsonb(a) - ARRAY['Id','Tipo','Cod','TipoAreaId'])
                OR t."TipoAreaId" <> v.area_id
        ) THEN
        RAISE EXCEPTION 'El catálogo no quedó completo o cambió un arancel.';
    END IF;
END
$separacion$;
