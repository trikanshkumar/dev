CREATE OR REPLACE PROCEDURE sp_update_load_ref(
    IN p_staging_table text,
    OUT errornumber text,
    OUT errorstate text,
    OUT errorprocedure text,
    OUT errorline text,
    OUT errormessage text)
LANGUAGE plpgsql
AS $BODY$
DECLARE
    sql_mark_complete text;
BEGIN
    -- Mark staging rows as completed after merge success.
    -- load_ref_te is already populated during staging COPY batch
    -- and propagated to main tables by the merge stored procedures.
    sql_mark_complete := format('UPDATE %I SET is_completed_ir = 1 WHERE is_completed_ir IS DISTINCT FROM 1', p_staging_table);
    EXECUTE sql_mark_complete;

    errornumber    := NULL;
    errorstate     := NULL;
    errorprocedure := 'sp_update_load_ref';
    errorline      := NULL;
    errormessage   := NULL;
EXCEPTION WHEN OTHERS THEN
    errornumber    := SQLSTATE;
    errorstate     := SQLSTATE;
    errorprocedure := 'sp_update_load_ref';
    GET STACKED DIAGNOSTICS errorline = PG_EXCEPTION_CONTEXT;
    errormessage   := SQLERRM;
END;
$BODY$;