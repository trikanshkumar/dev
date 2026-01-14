CREATE OR REPLACE PROCEDURE sp_destinationzipsvcasyvalidation_merge_proc(
    OUT insertcount integer,
    OUT updatecount integer,
    OUT deletecount integer,
    OUT errornumber text,
    OUT errorstate text,
    OUT errorprocedure text,
    OUT errorline text,
    OUT errormessage text)
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    CREATE TEMP TABLE merge_actions (
        table_name text,
        action     text,
        cny_cd text,
        dtn_psl_cd text,
        prc_pgm_prm_vlu_te decimal,
        rec_eff_stt_dt date,
        rec_eff_end_dt date
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               cny_cd,
               dtn_psl_cd,
               prc_pgm_prm_vlu_te,
               rec_eff_stt_dt,
               rec_eff_end_dt,
               load_ref_te
          FROM tdstsvp_stg
    ),
    cc_merge AS (
        MERGE INTO tdstsvp AS tgt
        USING src_dedup AS src
        ON (
            tgt.cny_cd             = src.cny_cd AND
            tgt.dtn_psl_cd         = src.dtn_psl_cd AND
            tgt.rec_eff_stt_dt     = src.rec_eff_stt_dt
        )
        WHEN MATCHED AND (
            tgt.prc_pgm_prm_vlu_te  IS DISTINCT FROM src.prc_pgm_prm_vlu_te OR
            tgt.rec_eff_end_dt      IS DISTINCT FROM src.rec_eff_end_dt OR
            tgt.load_ref_te         IS DISTINCT FROM src.load_ref_te
        )
            THEN UPDATE SET
                prc_pgm_prm_vlu_te  = src.prc_pgm_prm_vlu_te,
                rec_eff_end_dt      = src.rec_eff_end_dt,
                load_ref_te         = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
               cny_cd,
               dtn_psl_cd,
               prc_pgm_prm_vlu_te,
               rec_eff_stt_dt,
               rec_eff_end_dt,
               load_ref_te
            ) VALUES (
                src.cny_cd,
                src.dtn_psl_cd,
                src.prc_pgm_prm_vlu_te,
                src.rec_eff_stt_dt,
                src.rec_eff_end_dt,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(), 
            COALESCE(tgt.cny_cd,             src.cny_cd)             AS cny_cd,
            COALESCE(tgt.dtn_psl_cd,         src.dtn_psl_cd)         AS dtn_psl_cd,
            COALESCE(tgt.prc_pgm_prm_vlu_te, src.prc_pgm_prm_vlu_te) AS prc_pgm_prm_vlu_te,
            COALESCE(tgt.rec_eff_stt_dt,     src.rec_eff_stt_dt)     AS rec_eff_stt_dt,
            COALESCE(tgt.rec_eff_end_dt,     src.rec_eff_end_dt)     AS rec_eff_end_dt
    )
    INSERT INTO merge_actions (
        table_name, action,
        cny_cd, dtn_psl_cd, prc_pgm_prm_vlu_te, rec_eff_stt_dt, rec_eff_end_dt
    )
    SELECT
        'tdstsvp', merge_action,
        cny_cd, dtn_psl_cd, prc_pgm_prm_vlu_te, rec_eff_stt_dt, rec_eff_end_dt
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_destinationzipsvcasyvalidation_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_destinationzipsvcasyvalidation_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;