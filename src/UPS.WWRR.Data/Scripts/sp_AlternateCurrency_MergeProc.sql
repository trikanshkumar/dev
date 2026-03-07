CREATE OR REPLACE PROCEDURE sp_alternatecurrency_merge_proc(
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
        xpt_cny_cd text,
        cnv_fr_ccy_cd text,
        cnv_to_ccy_cd text,
        alt_ccy_xch_stt_dt date,
        alt_ccy_xch_end_dt date
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               xpt_cny_cd,
               cnv_fr_ccy_cd,
               cnv_to_ccy_cd,
               alt_ccy_xch_stt_dt,
               alt_ccy_xch_end_dt,
               alt_ccy_xch_ra_qy,
               alt_ccy_xch_or_qy,
               alt_ccy_dmc_ccl_qy,
               alt_ccy_rou_dmc_qy,
               usr_nr,
               load_ref_te
          FROM taltccy_stg
    ),
    cc_merge AS (
        MERGE INTO taltccy AS tgt
        USING src_dedup AS src
        ON (
            tgt.xpt_cny_cd         = src.xpt_cny_cd AND
            tgt.cnv_fr_ccy_cd      = src.cnv_fr_ccy_cd AND
            tgt.cnv_to_ccy_cd      = src.cnv_to_ccy_cd AND
            tgt.alt_ccy_xch_stt_dt = src.alt_ccy_xch_stt_dt AND
            tgt.alt_ccy_xch_end_dt = src.alt_ccy_xch_end_dt
        )
        WHEN MATCHED AND (
            tgt.alt_ccy_xch_ra_qy  IS DISTINCT FROM src.alt_ccy_xch_ra_qy OR
            tgt.alt_ccy_xch_or_qy  IS DISTINCT FROM src.alt_ccy_xch_or_qy OR
            tgt.alt_ccy_dmc_ccl_qy IS DISTINCT FROM src.alt_ccy_dmc_ccl_qy OR
            tgt.alt_ccy_rou_dmc_qy IS DISTINCT FROM src.alt_ccy_rou_dmc_qy OR
            tgt.usr_nr             IS DISTINCT FROM src.usr_nr
        )
            THEN UPDATE SET
                alt_ccy_xch_ra_qy  = src.alt_ccy_xch_ra_qy,
                alt_ccy_xch_or_qy  = src.alt_ccy_xch_or_qy,
                alt_ccy_dmc_ccl_qy = src.alt_ccy_dmc_ccl_qy,
                alt_ccy_rou_dmc_qy = src.alt_ccy_rou_dmc_qy,
                usr_nr             = src.usr_nr,
                load_ref_te        = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                xpt_cny_cd,
                cnv_fr_ccy_cd,
                cnv_to_ccy_cd,
                alt_ccy_xch_stt_dt,
                alt_ccy_xch_end_dt,
                alt_ccy_xch_ra_qy,
                alt_ccy_xch_or_qy,
                alt_ccy_dmc_ccl_qy,
                alt_ccy_rou_dmc_qy,
                usr_nr,
                load_ref_te
            ) VALUES (
                src.xpt_cny_cd,
                src.cnv_fr_ccy_cd,
                src.cnv_to_ccy_cd,
                src.alt_ccy_xch_stt_dt,
                src.alt_ccy_xch_end_dt,
                src.alt_ccy_xch_ra_qy,
                src.alt_ccy_xch_or_qy,
                src.alt_ccy_dmc_ccl_qy,
                src.alt_ccy_rou_dmc_qy,
                src.usr_nr,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.xpt_cny_cd,         src.xpt_cny_cd)         AS xpt_cny_cd,
            COALESCE(tgt.cnv_fr_ccy_cd,      src.cnv_fr_ccy_cd)      AS cnv_fr_ccy_cd,
            COALESCE(tgt.cnv_to_ccy_cd,      src.cnv_to_ccy_cd)      AS cnv_to_ccy_cd,
            COALESCE(tgt.alt_ccy_xch_stt_dt, src.alt_ccy_xch_stt_dt) AS alt_ccy_xch_stt_dt,
            COALESCE(tgt.alt_ccy_xch_end_dt, src.alt_ccy_xch_end_dt) AS alt_ccy_xch_end_dt
    )
    INSERT INTO merge_actions (
        table_name, action,
        xpt_cny_cd, cnv_fr_ccy_cd, cnv_to_ccy_cd, alt_ccy_xch_stt_dt, alt_ccy_xch_end_dt
    )
    SELECT
        'taltccy', merge_action,
        xpt_cny_cd, cnv_fr_ccy_cd, cnv_to_ccy_cd, alt_ccy_xch_stt_dt, alt_ccy_xch_end_dt
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_alternatecurrency_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_alternatecurrency_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;