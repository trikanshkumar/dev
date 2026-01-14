CREATE OR REPLACE PROCEDURE sp_simpleratevolumerange_merge_proc(
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
        pkg_cha_typ_cd char(3),
        bil_ter_typ_cd char(3),
        svc_fea_typ_cd char(3),
        svc_typ_cd char(3),
        mvm_drc_cd char(1),
        cus_csf_typ_cd char(2),
        wgt_ms_unt_typ_cd char(2),
        apv_sts_cd char(2),
        rec_eff_stt_dt date,
        rec_eff_end_dt date,
        vol_rng_min_qy decimal(13,2),
        vol_rng_max_qy decimal(13,2),
        ms_unt_typ_cd char(3),
        dw_min_qy decimal(13,2),
        dw_max_qy decimal(13,2),
        pbh_max_wgt_qy decimal(13,2)
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               gpn_xpt_cny_cd,
               gpn_ipt_cny_cd,
               pkg_cha_typ_cd,
               bil_ter_typ_cd,
               svc_fea_typ_cd,
               svc_typ_cd,
               mvm_drc_cd,
               cus_csf_typ_cd,
               wgt_ms_unt_typ_cd,
               apv_sts_cd,
               rec_eff_stt_dt,
               rec_eff_end_dt,
               vol_rng_min_qy,
               vol_rng_max_qy,
               ms_unt_typ_cd,
               dw_min_qy,
               dw_max_qy,
               pbh_max_wgt_qy,
               load_ref_te
          FROM tsiarav_stg
    ),
    cc_merge AS (
        MERGE INTO tsiarav AS tgt
        USING src_dedup AS src
        ON (
            tgt.gpn_xpt_cny_cd   = src.gpn_xpt_cny_cd AND
            tgt.gpn_ipt_cny_cd   = src.gpn_ipt_cny_cd AND
            tgt.pkg_cha_typ_cd   = src.pkg_cha_typ_cd AND
            tgt.bil_ter_typ_cd   = src.bil_ter_typ_cd AND
            tgt.svc_fea_typ_cd   = src.svc_fea_typ_cd AND
            tgt.svc_typ_cd       = src.svc_typ_cd AND
            tgt.mvm_drc_cd       = src.mvm_drc_cd AND
            tgt.cus_csf_typ_cd   = src.cus_csf_typ_cd AND
            tgt.wgt_ms_unt_typ_cd= src.wgt_ms_unt_typ_cd AND
            tgt.apv_sts_cd       = src.apv_sts_cd AND
            tgt.rec_eff_stt_dt   = src.rec_eff_stt_dt
        )
        WHEN MATCHED AND (
            tgt.rec_eff_end_dt  IS DISTINCT FROM src.rec_eff_end_dt OR
            tgt.vol_rng_min_qy  IS DISTINCT FROM src.vol_rng_min_qy OR
            tgt.vol_rng_max_qy  IS DISTINCT FROM src.vol_rng_max_qy OR
            tgt.ms_unt_typ_cd   IS DISTINCT FROM src.ms_unt_typ_cd OR
            tgt.dw_min_qy       IS DISTINCT FROM src.dw_min_qy OR
            tgt.dw_max_qy       IS DISTINCT FROM src.dw_max_qy OR
            tgt.pbh_max_wgt_qy  IS DISTINCT FROM src.pbh_max_wgt_qy OR
            tgt.load_ref_te     IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            rec_eff_end_dt = src.rec_eff_end_dt,
            vol_rng_min_qy = src.vol_rng_min_qy,
            vol_rng_max_qy = src.vol_rng_max_qy,
            ms_unt_typ_cd  = src.ms_unt_typ_cd,
            dw_min_qy      = src.dw_min_qy,
            dw_max_qy      = src.dw_max_qy,
            pbh_max_wgt_qy = src.pbh_max_wgt_qy,
            load_ref_te    = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                gpn_xpt_cny_cd,
                gpn_ipt_cny_cd,
                pkg_cha_typ_cd,
                bil_ter_typ_cd,
                svc_fea_typ_cd,
                svc_typ_cd,
                mvm_drc_cd,
                cus_csf_typ_cd,
                wgt_ms_unt_typ_cd,
                apv_sts_cd,
                rec_eff_stt_dt,
                rec_eff_end_dt,
                vol_rng_min_qy,
                vol_rng_max_qy,
                ms_unt_typ_cd,
                dw_min_qy,
                dw_max_qy,
                pbh_max_wgt_qy,
                load_ref_te
            ) VALUES (
                src.gpn_xpt_cny_cd,
                src.gpn_ipt_cny_cd,
                src.pkg_cha_typ_cd,
                src.bil_ter_typ_cd,
                src.svc_fea_typ_cd,
                src.svc_typ_cd,
                src.mvm_drc_cd,
                src.cus_csf_typ_cd,
                src.wgt_ms_unt_typ_cd,
                src.apv_sts_cd,
                src.rec_eff_stt_dt,
                src.rec_eff_end_dt,
                src.vol_rng_min_qy,
                src.vol_rng_max_qy,
                src.ms_unt_typ_cd,
                src.dw_min_qy,
                src.dw_max_qy,
                src.pbh_max_wgt_qy,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.gpn_xpt_cny_cd,    src.gpn_xpt_cny_cd)   AS gpn_xpt_cny_cd,
            COALESCE(tgt.gpn_ipt_cny_cd,    src.gpn_ipt_cny_cd)   AS gpn_ipt_cny_cd,
            COALESCE(tgt.pkg_cha_typ_cd,    src.pkg_cha_typ_cd)   AS pkg_cha_typ_cd,
            COALESCE(tgt.bil_ter_typ_cd,    src.bil_ter_typ_cd)   AS bil_ter_typ_cd,
            COALESCE(tgt.svc_fea_typ_cd,    src.svc_fea_typ_cd)   AS svc_fea_typ_cd,
            COALESCE(tgt.svc_typ_cd,        src.svc_typ_cd)       AS svc_typ_cd,
            COALESCE(tgt.mvm_drc_cd,        src.mvm_drc_cd)       AS mvm_drc_cd,
            COALESCE(tgt.cus_csf_typ_cd,    src.cus_csf_typ_cd)   AS cus_csf_typ_cd,
            COALESCE(tgt.wgt_ms_unt_typ_cd, src.wgt_ms_unt_typ_cd)AS wgt_ms_unt_typ_cd,
            COALESCE(tgt.apv_sts_cd,        src.apv_sts_cd)       AS apv_sts_cd,
            COALESCE(tgt.rec_eff_stt_dt,    src.rec_eff_stt_dt)   AS rec_eff_stt_dt,
            COALESCE(tgt.rec_eff_end_dt,    src.rec_eff_end_dt)   AS rec_eff_end_dt,
            COALESCE(tgt.vol_rng_min_qy,    src.vol_rng_min_qy)   AS vol_rng_min_qy,
            COALESCE(tgt.vol_rng_max_qy,    src.vol_rng_max_qy)   AS vol_rng_max_qy,
            COALESCE(tgt.ms_unt_typ_cd,     src.ms_unt_typ_cd)    AS ms_unt_typ_cd,
            COALESCE(tgt.dw_min_qy,         src.dw_min_qy)        AS dw_min_qy,
            COALESCE(tgt.dw_max_qy,         src.dw_max_qy)        AS dw_max_qy,
            COALESCE(tgt.pbh_max_wgt_qy,    src.pbh_max_wgt_qy)   AS pbh_max_wgt_qy
    )
    INSERT INTO merge_actions (
        table_name, action,
        gpn_xpt_cny_cd, gpn_ipt_cny_cd, pkg_cha_typ_cd, bil_ter_typ_cd, svc_fea_typ_cd, svc_typ_cd,
        mvm_drc_cd, cus_csf_typ_cd, wgt_ms_unt_typ_cd, apv_sts_cd, rec_eff_stt_dt,
        rec_eff_end_dt, vol_rng_min_qy, vol_rng_max_qy, ms_unt_typ_cd, dw_min_qy, dw_max_qy, pbh_max_wgt_qy
    )
    SELECT 'tsiarav', merge_action,
           gpn_xpt_cny_cd, gpn_ipt_cny_cd, pkg_cha_typ_cd, bil_ter_typ_cd, svc_fea_typ_cd, svc_typ_cd,
           mvm_drc_cd, cus_csf_typ_cd, wgt_ms_unt_typ_cd, apv_sts_cd, rec_eff_stt_dt,
            rec_eff_end_dt, vol_rng_min_qy, vol_rng_max_qy, ms_unt_typ_cd, dw_min_qy, dw_max_qy, pbh_max_wgt_qy
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_simpleratevolumerange_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_simpleratevolumerange_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;