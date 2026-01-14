CREATE OR REPLACE PROCEDURE sp_update_load_ref(
    IN p_staging_table text,
    IN p_main_table text,
    IN p_load_ref_te text,
    OUT errornumber text,
    OUT errorstate text,
    OUT errorprocedure text,
    OUT errorline text,
    OUT errormessage text)
LANGUAGE plpgsql
AS $BODY$
DECLARE
    sql_mark_complete text;
    sql_stg text;
    sql_main text;
BEGIN
    -- Mark staging rows as completed after merge success
    sql_mark_complete := format('UPDATE %I SET is_completed_ir = 1 WHERE is_completed_ir IS DISTINCT FROM 1', p_staging_table);
    EXECUTE sql_mark_complete;

    -- Build dynamic SQL for staging table load_ref update
    sql_stg := format('UPDATE %I SET load_ref_te = $1 WHERE is_completed_ir = 1 AND (load_ref_te IS NULL OR load_ref_te = '''')', p_staging_table);
    EXECUTE sql_stg USING p_load_ref_te;

    -- Build dynamic SQL for main table update (no is_completed_ir assumed)
    sql_main := format('UPDATE %I SET load_ref_te = $1 WHERE (load_ref_te IS NULL OR load_ref_te = '''')', p_main_table);
    EXECUTE sql_main USING p_load_ref_te;

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