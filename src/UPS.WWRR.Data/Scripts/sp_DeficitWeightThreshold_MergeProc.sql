CREATE OR REPLACE PROCEDURE sp_deficitweightthreshold_merge_proc(
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
        wgt_ms_unt_typ_cd text,
        apv_sts_cd text,
        rec_eff_stt_dt date,
        rec_eff_end_dt date
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               cny_cd,
               wgt_ms_unt_typ_cd,
               dfw_rtg_min_wgt_qy,
               wgt_dat_ppn_ir,
               apv_sts_cd,
               rec_eff_stt_dt,
               rec_eff_end_dt
          FROM tdfwthr_stg
    ),
    cc_merge AS (
        MERGE INTO tdfwthr AS tgt
        USING src_dedup AS src
        ON (
            tgt.cny_cd             = src.cny_cd AND
            tgt.wgt_ms_unt_typ_cd  = src.wgt_ms_unt_typ_cd AND
            tgt.apv_sts_cd         = src.apv_sts_cd AND
            tgt.rec_eff_stt_dt     = src.rec_eff_stt_dt
        )
        WHEN MATCHED AND (
            tgt.dfw_rtg_min_wgt_qy IS DISTINCT FROM src.dfw_rtg_min_wgt_qy OR
            tgt.wgt_dat_ppn_ir     IS DISTINCT FROM src.wgt_dat_ppn_ir OR
            tgt.rec_eff_end_dt     IS DISTINCT FROM src.rec_eff_end_dt
        )
            THEN UPDATE SET
                dfw_rtg_min_wgt_qy = src.dfw_rtg_min_wgt_qy,
                wgt_dat_ppn_ir     = src.wgt_dat_ppn_ir,
                rec_eff_end_dt     = src.rec_eff_end_dt
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                cny_cd,
                wgt_ms_unt_typ_cd,
                dfw_rtg_min_wgt_qy,
                wgt_dat_ppn_ir,
                apv_sts_cd,
                rec_eff_stt_dt,
                rec_eff_end_dt
            ) VALUES (
                src.cny_cd,
                src.wgt_ms_unt_typ_cd,
                src.dfw_rtg_min_wgt_qy,
                src.wgt_dat_ppn_ir,
                src.apv_sts_cd,
                src.rec_eff_stt_dt,
                src.rec_eff_end_dt
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.cny_cd,            src.cny_cd)            AS cny_cd,
            COALESCE(tgt.wgt_ms_unt_typ_cd, src.wgt_ms_unt_typ_cd) AS wgt_ms_unt_typ_cd,
            COALESCE(tgt.apv_sts_cd,        src.apv_sts_cd)        AS apv_sts_cd,
            COALESCE(tgt.rec_eff_stt_dt,    src.rec_eff_stt_dt)    AS rec_eff_stt_dt,
            COALESCE(tgt.rec_eff_end_dt,    src.rec_eff_end_dt)    AS rec_eff_end_dt
    )
    INSERT INTO merge_actions (
        table_name, action,
        cny_cd, wgt_ms_unt_typ_cd, apv_sts_cd, rec_eff_stt_dt, rec_eff_end_dt
    )
    SELECT
        'tdfwthr', merge_action,
        cny_cd, wgt_ms_unt_typ_cd, apv_sts_cd, rec_eff_stt_dt, rec_eff_end_dt
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_deficitweightthreshold_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_deficitweightthreshold_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;