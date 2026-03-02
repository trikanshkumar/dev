CREATE OR REPLACE PROCEDURE sp_fuelsurchargecategorymap_merge_proc(
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
        gpn_xpt_cny_cd char(4),
        gpn_ipt_cny_cd char(4),
        mvm_drc_cd char(1),
        svc_typ_cd char(3),
        cus_csf_typ_cd char(2),
        ccy_cd char(3),
        apv_sts_cd char(2),
        rec_eff_stt_dt date
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               gpn_xpt_cny_cd,
               gpn_ipt_cny_cd,
               mvm_drc_cd,
               svc_typ_cd,
               cus_csf_typ_cd,
               ccy_cd,
               apv_sts_cd,
               rec_eff_stt_dt,
               rec_eff_end_dt,
               pse_idx_fu_cgy_cd,
               rec_ins_ts,
               load_ref_te
          FROM tfscmap_stg
    ),
    cc_merge AS (
        MERGE INTO tfscmap AS tgt
        USING src_dedup AS src
        ON (
            tgt.gpn_xpt_cny_cd   = src.gpn_xpt_cny_cd AND
            tgt.gpn_ipt_cny_cd   = src.gpn_ipt_cny_cd AND
            tgt.mvm_drc_cd       = src.mvm_drc_cd AND
            tgt.svc_typ_cd       = src.svc_typ_cd AND
            tgt.cus_csf_typ_cd   = src.cus_csf_typ_cd AND
            tgt.ccy_cd           = src.ccy_cd AND
            tgt.apv_sts_cd       = src.apv_sts_cd AND
            tgt.rec_eff_stt_dt   = src.rec_eff_stt_dt
        )
        WHEN MATCHED AND (
            tgt.rec_eff_end_dt     IS DISTINCT FROM src.rec_eff_end_dt OR
            tgt.pse_idx_fu_cgy_cd  IS DISTINCT FROM src.pse_idx_fu_cgy_cd OR
            tgt.rec_ins_ts         IS DISTINCT FROM src.rec_ins_ts OR
            tgt.load_ref_te        IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            rec_eff_end_dt     = src.rec_eff_end_dt,
            pse_idx_fu_cgy_cd  = src.pse_idx_fu_cgy_cd,
            rec_ins_ts         = src.rec_ins_ts,
            load_ref_te        = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                gpn_xpt_cny_cd,
                gpn_ipt_cny_cd,
                mvm_drc_cd,
                svc_typ_cd,
                cus_csf_typ_cd,
                ccy_cd,
                apv_sts_cd,
                rec_eff_stt_dt,
                rec_eff_end_dt,
                pse_idx_fu_cgy_cd,
                rec_ins_ts,
                load_ref_te
            ) VALUES (
                src.gpn_xpt_cny_cd,
                src.gpn_ipt_cny_cd,
                src.mvm_drc_cd,
                src.svc_typ_cd,
                src.cus_csf_typ_cd,
                src.ccy_cd,
                src.apv_sts_cd,
                src.rec_eff_stt_dt,
                src.rec_eff_end_dt,
                src.pse_idx_fu_cgy_cd,
                src.rec_ins_ts,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.gpn_xpt_cny_cd, src.gpn_xpt_cny_cd) AS gpn_xpt_cny_cd,
            COALESCE(tgt.gpn_ipt_cny_cd, src.gpn_ipt_cny_cd) AS gpn_ipt_cny_cd,
            COALESCE(tgt.mvm_drc_cd,     src.mvm_drc_cd)     AS mvm_drc_cd,
            COALESCE(tgt.svc_typ_cd,     src.svc_typ_cd)     AS svc_typ_cd,
            COALESCE(tgt.cus_csf_typ_cd, src.cus_csf_typ_cd) AS cus_csf_typ_cd,
            COALESCE(tgt.ccy_cd,         src.ccy_cd)         AS ccy_cd,
            COALESCE(tgt.apv_sts_cd,     src.apv_sts_cd)     AS apv_sts_cd,
            COALESCE(tgt.rec_eff_stt_dt, src.rec_eff_stt_dt) AS rec_eff_stt_dt
    )
    INSERT INTO merge_actions (
        table_name,
        action,
        gpn_xpt_cny_cd,
        gpn_ipt_cny_cd,
        mvm_drc_cd,
        svc_typ_cd,
        cus_csf_typ_cd,
        ccy_cd,
        apv_sts_cd,
        rec_eff_stt_dt
    )
    SELECT
        'tfscmap',
        merge_action,
        gpn_xpt_cny_cd,
        gpn_ipt_cny_cd,
        mvm_drc_cd,
        svc_typ_cd,
        cus_csf_typ_cd,
        ccy_cd,
        apv_sts_cd,
        rec_eff_stt_dt
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_fuelsurchargecategorymap_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_fuelsurchargecategorymap_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
