CREATE OR REPLACE PROCEDURE sp_internationalratingcurrency_merge_proc(
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
        cny_cd char(2),
        rtg_ccy_cd char(3),
        rtg_ccy_stt_dt date,
        apv_sts_cd char(2)
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               cny_cd,
               rtg_ccy_cd,
               rtg_ccy_stt_dt,
               rtg_ccy_end_dt,
               apv_sts_cd,
               load_ref_te
          FROM tiraccy_stg
    ),
    cc_merge AS (
        MERGE INTO tiraccy AS tgt
        USING src_dedup AS src
        ON (
            tgt.cny_cd        = src.cny_cd AND
            tgt.rtg_ccy_cd    = src.rtg_ccy_cd AND
            tgt.rtg_ccy_stt_dt= src.rtg_ccy_stt_dt AND
            tgt.apv_sts_cd    = src.apv_sts_cd
        )
        WHEN MATCHED AND (
            tgt.rtg_ccy_end_dt IS DISTINCT FROM src.rtg_ccy_end_dt OR
            tgt.load_ref_te    IS DISTINCT FROM src.load_ref_te
        ) THEN
            UPDATE SET
                rtg_ccy_end_dt = src.rtg_ccy_end_dt,
                load_ref_te    = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                cny_cd,
                rtg_ccy_cd,
                rtg_ccy_stt_dt,
                rtg_ccy_end_dt,
                apv_sts_cd,
                load_ref_te
            ) VALUES (
                src.cny_cd,
                src.rtg_ccy_cd,
                src.rtg_ccy_stt_dt,
                src.rtg_ccy_end_dt,
                src.apv_sts_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.cny_cd,        src.cny_cd)        AS cny_cd,
            COALESCE(tgt.rtg_ccy_cd,    src.rtg_ccy_cd)    AS rtg_ccy_cd,
            COALESCE(tgt.rtg_ccy_stt_dt,src.rtg_ccy_stt_dt)AS rtg_ccy_stt_dt,
            COALESCE(tgt.apv_sts_cd,    src.apv_sts_cd)    AS apv_sts_cd
    )
    INSERT INTO merge_actions (
        table_name,
        action,
        cny_cd,
        rtg_ccy_cd,
        rtg_ccy_stt_dt,
        apv_sts_cd
    )
    SELECT
        'tiraccy',
        merge_action,
        cny_cd,
        rtg_ccy_cd,
        rtg_ccy_stt_dt,
        apv_sts_cd
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_internationalratingcurrency_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_internationalratingcurrency_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
