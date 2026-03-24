CREATE OR REPLACE PROCEDURE sp_ratechart_accessorialrates_merge_proc(
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
DECLARE
    v_insert_total integer := 0;
    v_update_total integer := 0;
    v_delete_total integer := 0;
    v_insert integer;
    v_update integer;
    v_delete integer;
BEGIN
    CREATE TEMP TABLE merge_actions (
        table_name text,
        action     text
    );

    -- NOTE: chartsts merge is handled in sp_ratechart_accessorialrates_stagingdataset_proc
    -- to ensure correct zch_sts_nr values are used for all dependent staging tables

    -- 1. MERGE accrate
    WITH src_dedup AS (
        SELECT zch_sts_nr,
               asy_svc_ra,
               del_zn_nr,
               ra_chg_csf_typ_cd,
               load_ref_te
          FROM accrate_stg
    ),
    accrate_merge AS (
        MERGE INTO accrate AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr        = src.zch_sts_nr AND
            tgt.asy_svc_ra        = src.asy_svc_ra AND
            tgt.del_zn_nr         = src.del_zn_nr AND
            tgt.ra_chg_csf_typ_cd = src.ra_chg_csf_typ_cd
        )
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                asy_svc_ra,
                del_zn_nr,
                ra_chg_csf_typ_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.asy_svc_ra,
                src.del_zn_nr,
                src.ra_chg_csf_typ_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'accrate', merge_action FROM accrate_merge;

    -- 2. MERGE accratecrit
    WITH src_dedup AS (
        SELECT zch_sts_nr,
               asy_svc_ra,
               dtr_cri_vlu_typ_cd,
               dtr_cri_lo_rng_te,
               dtr_cri_hi_rng_te,
               ccl_mth_typ_cd,
               load_ref_te
          FROM accratecrit_stg
    ),
    accratecrit_merge AS (
        MERGE INTO accratecrit AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr         = src.zch_sts_nr AND
            tgt.asy_svc_ra         = src.asy_svc_ra AND
            tgt.dtr_cri_vlu_typ_cd = src.dtr_cri_vlu_typ_cd AND
            tgt.dtr_cri_lo_rng_te  = src.dtr_cri_lo_rng_te AND
            tgt.ccl_mth_typ_cd     = src.ccl_mth_typ_cd
        )
        WHEN MATCHED AND (
            tgt.dtr_cri_hi_rng_te IS DISTINCT FROM src.dtr_cri_hi_rng_te 
        ) THEN UPDATE SET
            dtr_cri_hi_rng_te = src.dtr_cri_hi_rng_te,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                asy_svc_ra,
                dtr_cri_vlu_typ_cd,
                dtr_cri_lo_rng_te,
                dtr_cri_hi_rng_te,
                ccl_mth_typ_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.asy_svc_ra,
                src.dtr_cri_vlu_typ_cd,
                src.dtr_cri_lo_rng_te,
                src.dtr_cri_hi_rng_te,
                src.ccl_mth_typ_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'accratecrit', merge_action FROM accratecrit_merge;

    -- 3. MERGE chartsvcpkg
    WITH src_dedup AS (
        SELECT zch_sts_nr,
               svc_typ_cd,
               pkg_cha_typ_cd,
               svc_fea_typ_cd,
               pkg_acq_mth_typ_cd,
               na_nrs_cd,
               svc_ra_cht_seq_nr,
               pkg_acq_mth_csf_cd,
               load_ref_te
          FROM chartsvcpkg_stg
    ),
    chartsvcpkg_merge AS (
        MERGE INTO chartsvcpkg AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr =         src.zch_sts_nr AND
            tgt.svc_typ_cd =         src.svc_typ_cd AND
            tgt.pkg_cha_typ_cd =     src.pkg_cha_typ_cd AND
            tgt.svc_fea_typ_cd =     src.svc_fea_typ_cd AND
            tgt.pkg_acq_mth_typ_cd = src.pkg_acq_mth_typ_cd AND
            tgt.na_nrs_cd =          src.na_nrs_cd
        )
        WHEN MATCHED AND (
            tgt.svc_ra_cht_seq_nr  IS DISTINCT FROM src.svc_ra_cht_seq_nr OR
            tgt.pkg_acq_mth_csf_cd IS DISTINCT FROM src.pkg_acq_mth_csf_cd 
        ) THEN UPDATE SET
            svc_ra_cht_seq_nr  = src.svc_ra_cht_seq_nr,
            pkg_acq_mth_csf_cd = src.pkg_acq_mth_csf_cd,
            load_ref_te        = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                svc_typ_cd,
                pkg_cha_typ_cd,
                svc_fea_typ_cd,
                pkg_acq_mth_typ_cd,
                na_nrs_cd,
                svc_ra_cht_seq_nr,
                pkg_acq_mth_csf_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.svc_typ_cd,
                src.pkg_cha_typ_cd,
                src.svc_fea_typ_cd,
                src.pkg_acq_mth_typ_cd,
                src.na_nrs_cd,
                src.svc_ra_cht_seq_nr,
                src.pkg_acq_mth_csf_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'chartvsvcpkg', merge_action FROM chartsvcpkg_merge;

    -- 4. MERGE chartorggeo
    WITH src_dedup AS (
        SELECT zch_sts_nr,
               xpt_cny_cd,
               gpu_xpt_cny_cd,
               ipt_cny_cd,
               gpu_ipt_cny_cd,
               svc_typ_cd,
               pkg_cha_typ_cd,
               cus_cls_typ_cd,
               load_ref_te
          FROM chartorggeo_stg
    ),
    chartorggeo_merge AS (
        MERGE INTO chartorggeo AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr     = src.zch_sts_nr AND
            tgt.gpu_xpt_cny_cd = src.gpu_xpt_cny_cd AND
            tgt.gpu_ipt_cny_cd = src.gpu_ipt_cny_cd AND
            tgt.svc_typ_cd     = src.svc_typ_cd AND
            tgt.pkg_cha_typ_cd = src.pkg_cha_typ_cd AND
            tgt.cus_cls_typ_cd = src.cus_cls_typ_cd
        )
        WHEN MATCHED AND (
            tgt.xpt_cny_cd  IS DISTINCT FROM src.xpt_cny_cd OR
            tgt.ipt_cny_cd  IS DISTINCT FROM src.ipt_cny_cd 
        ) THEN UPDATE SET
            xpt_cny_cd  = src.xpt_cny_cd,
            ipt_cny_cd  = src.ipt_cny_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                xpt_cny_cd,
                gpu_xpt_cny_cd,
                ipt_cny_cd,
                gpu_ipt_cny_cd,
                svc_typ_cd,
                pkg_cha_typ_cd,
                cus_cls_typ_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.xpt_cny_cd,
                src.gpu_xpt_cny_cd,
                src.ipt_cny_cd,
                src.gpu_ipt_cny_cd,
                src.svc_typ_cd,
                src.pkg_cha_typ_cd,
                src.cus_cls_typ_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'chartorggeo', merge_action FROM chartorggeo_merge;

    -- 5. MERGE chartacccd
    WITH src_dedup AS (
        SELECT zch_sts_nr,
               asy_svc_typ_cd,
               bil_ter_typ_cd,
               mvm_drc_cd,
               ccy_cd,
               load_ref_te
          FROM chartacccd_stg
    ),
    chartacccd_merge AS (
        MERGE INTO chartacccd AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr
        )
        WHEN MATCHED AND (
            tgt.asy_svc_typ_cd IS DISTINCT FROM src.asy_svc_typ_cd OR
            tgt.bil_ter_typ_cd IS DISTINCT FROM src.bil_ter_typ_cd OR
            tgt.mvm_drc_cd     IS DISTINCT FROM src.mvm_drc_cd OR
            tgt.ccy_cd         IS DISTINCT FROM src.ccy_cd 
        ) THEN UPDATE SET
            asy_svc_typ_cd = src.asy_svc_typ_cd,
            bil_ter_typ_cd = src.bil_ter_typ_cd,
            mvm_drc_cd     = src.mvm_drc_cd,
            ccy_cd         = src.ccy_cd,
            load_ref_te    = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                asy_svc_typ_cd,
                bil_ter_typ_cd,
                mvm_drc_cd,
                ccy_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.asy_svc_typ_cd,
                src.bil_ter_typ_cd,
                src.mvm_drc_cd,
                src.ccy_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'chartacccd', merge_action FROM chartacccd_merge;

    -- Aggregate counts across all tables
    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO insertcount, updatecount, deletecount
    FROM merge_actions;

    errornumber    := NULL;
    errorstate     := NULL;
    errorprocedure := 'sp_ratechart_accessorialrates_merge_proc';
    errorline      := NULL;
    errormessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    errornumber    := SQLSTATE;
    errorstate     := SQLSTATE;
    errorprocedure := 'sp_ratechart_accessorialrates_merge_proc';
    GET STACKED DIAGNOSTICS errorline = PG_EXCEPTION_CONTEXT;
    errormessage   := SQLERRM;
    insertcount := 0;
    updatecount := 0;
    deletecount := 0;
END;
$BODY$;
