CREATE OR REPLACE PROCEDURE sp_update_load_ref(
    IN  p_staging_table text,
    IN  p_main_table    text,
    IN  p_load_ref_te   text,
    OUT errornumber     text,
    OUT errorstate      text,
    OUT errorprocedure  text,
    OUT errorline       text,
    OUT errormessage    text
)
LANGUAGE plpgsql
AS $BODY$
DECLARE
    sql_stg  text;
    sql_main text;
    v_upd_stg  bigint;
    v_upd_main bigint;
BEGIN
    /*
      Combine the staging updates into a single statement:
      - Mark rows completed
      - Set load_ref_te
      Only touch rows that actually need a change.
    */
    sql_stg := format(
        'UPDATE %I
         SET is_completed_ir = 1,
             load_ref_te     = $1
         WHERE is_completed_ir IS DISTINCT FROM 1
            OR load_ref_te   IS DISTINCT FROM $1',
        p_staging_table
    );
    EXECUTE sql_stg USING p_load_ref_te;
    GET DIAGNOSTICS v_upd_stg = ROW_COUNT;

    /*
      Update main table load_ref_te; avoid no-op updates as well.
      (Assumes main table has column load_ref_te and no is_completed_ir.)
    */
    sql_main := format(
        'UPDATE %I
         SET load_ref_te = $1
         WHERE load_ref_te IS DISTINCT FROM $1',
        p_main_table
    );
    EXECUTE sql_main USING p_load_ref_te;
    GET DIAGNOSTICS v_upd_main = ROW_COUNT;

    -- (Optional) Emit notices; you can remove these if you prefer quiet runs.
    RAISE NOTICE '[%] sp_update_load_ref: staging updated rows: %, main updated rows: %',
        clock_timestamp(), v_upd_stg, v_upd_main;

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