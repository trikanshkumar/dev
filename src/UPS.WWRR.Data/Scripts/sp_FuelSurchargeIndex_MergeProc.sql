CREATE OR REPLACE PROCEDURE sp_fuelsurchargeindex_merge_proc(
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
        pse_idx_fu_cgy_cd char(2),
        rec_eff_stt_dt date,
        apv_sts_cd char(2),
        rec_eff_end_dt date,
        pse_idx_fu_ra_pr numeric(18,4),
        ccy_cd char(3),
        fu_ms_unt_typ_cd char(2),
        pse_idx_fu_cgy_te char(100),
        fu_sur_pbh_rt_a numeric(17,4)
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               pse_idx_fu_cgy_cd,
               rec_eff_stt_dt,
               apv_sts_cd,
               rec_eff_end_dt,
               pse_idx_fu_ra_pr,
               ccy_cd,
               fu_ms_unt_typ_cd,
               pse_idx_fu_cgy_te,
               fu_sur_pbh_rt_a,
               load_ref_te
          FROM tfscidx_stg
    ),
    cc_merge AS (
        MERGE INTO tfscidx AS tgt
        USING src_dedup AS src
        ON (
            tgt.pse_idx_fu_cgy_cd = src.pse_idx_fu_cgy_cd AND
            tgt.rec_eff_stt_dt    = src.rec_eff_stt_dt AND
            tgt.apv_sts_cd        = src.apv_sts_cd
        )
        WHEN MATCHED AND (
            tgt.rec_eff_end_dt     IS DISTINCT FROM src.rec_eff_end_dt OR
            tgt.pse_idx_fu_ra_pr   IS DISTINCT FROM src.pse_idx_fu_ra_pr OR
            tgt.ccy_cd             IS DISTINCT FROM src.ccy_cd OR
            tgt.fu_ms_unt_typ_cd   IS DISTINCT FROM src.fu_ms_unt_typ_cd OR
            tgt.pse_idx_fu_cgy_te  IS DISTINCT FROM src.pse_idx_fu_cgy_te OR
            tgt.fu_sur_pbh_rt_a    IS DISTINCT FROM src.fu_sur_pbh_rt_a
        ) THEN UPDATE SET
            rec_eff_end_dt     = src.rec_eff_end_dt,
            pse_idx_fu_ra_pr   = src.pse_idx_fu_ra_pr,
            ccy_cd             = src.ccy_cd,
            fu_ms_unt_typ_cd   = src.fu_ms_unt_typ_cd,
            pse_idx_fu_cgy_te  = src.pse_idx_fu_cgy_te,
            fu_sur_pbh_rt_a    = src.fu_sur_pbh_rt_a,
            load_ref_te        = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                pse_idx_fu_cgy_cd,
                rec_eff_stt_dt,
                apv_sts_cd,
                rec_eff_end_dt,
                pse_idx_fu_ra_pr,
                ccy_cd,
                fu_ms_unt_typ_cd,
                pse_idx_fu_cgy_te,
                fu_sur_pbh_rt_a,
                load_ref_te
            ) VALUES (
                src.pse_idx_fu_cgy_cd,
                src.rec_eff_stt_dt,
                src.apv_sts_cd,
                src.rec_eff_end_dt,
                src.pse_idx_fu_ra_pr,
                src.ccy_cd,
                src.fu_ms_unt_typ_cd,
                src.pse_idx_fu_cgy_te,
                src.fu_sur_pbh_rt_a,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.pse_idx_fu_cgy_cd, src.pse_idx_fu_cgy_cd) AS pse_idx_fu_cgy_cd,
            COALESCE(tgt.rec_eff_stt_dt,    src.rec_eff_stt_dt)    AS rec_eff_stt_dt,
            COALESCE(tgt.apv_sts_cd,        src.apv_sts_cd)        AS apv_sts_cd,
            COALESCE(tgt.rec_eff_end_dt,    src.rec_eff_end_dt)    AS rec_eff_end_dt,
            COALESCE(tgt.pse_idx_fu_ra_pr,  src.pse_idx_fu_ra_pr)  AS pse_idx_fu_ra_pr,
            COALESCE(tgt.ccy_cd,            src.ccy_cd)            AS ccy_cd,
            COALESCE(tgt.fu_ms_unt_typ_cd,  src.fu_ms_unt_typ_cd)  AS fu_ms_unt_typ_cd,
            COALESCE(tgt.pse_idx_fu_cgy_te, src.pse_idx_fu_cgy_te) AS pse_idx_fu_cgy_te,
            COALESCE(tgt.fu_sur_pbh_rt_a,   src.fu_sur_pbh_rt_a)   AS fu_sur_pbh_rt_a
    )
    INSERT INTO merge_actions (
        table_name,
        action,
        pse_idx_fu_cgy_cd,
        rec_eff_stt_dt,
        apv_sts_cd,
        rec_eff_end_dt,
        pse_idx_fu_ra_pr,
        ccy_cd,
        fu_ms_unt_typ_cd,
        pse_idx_fu_cgy_te,
        fu_sur_pbh_rt_a
    )
    SELECT
        'tfscidx',
        merge_action,
        pse_idx_fu_cgy_cd,
        rec_eff_stt_dt,
        apv_sts_cd,
        rec_eff_end_dt,
        pse_idx_fu_ra_pr,
        ccy_cd,
        fu_ms_unt_typ_cd,
        pse_idx_fu_cgy_te,
        fu_sur_pbh_rt_a
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_fuelsurchargeindex_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_fuelsurchargeindex_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
