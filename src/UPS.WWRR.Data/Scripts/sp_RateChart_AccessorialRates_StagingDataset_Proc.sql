CREATE OR REPLACE PROCEDURE sp_ratechart_accessorialrates_stagingdataset_proc(
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
	v_chartsts_ins integer := 0;
	v_chartsts_upd integer := 0;
	v_chartsts_del integer := 0;
	v_accrate_ins integer;
	v_accratecrit_ins integer;
	v_chartsvcpkg_ins integer;
	v_chartorggeo_ins integer;
	v_chartacccd_ins integer;
BEGIN
    -- Clear normalized staging tables before populating
    TRUNCATE TABLE chartsts_stg;
    TRUNCATE TABLE accrate_stg;
    TRUNCATE TABLE accratecrit_stg;
	TRUNCATE TABLE chartsvcpkg_stg;
    TRUNCATE TABLE chartorggeo_stg;
    TRUNCATE TABLE chartacccd_stg;

    

    -- Create temp table to track merge actions for chartsts
    CREATE TEMP TABLE IF NOT EXISTS chartsts_merge_actions (
        action text
    );
    TRUNCATE TABLE chartsts_merge_actions;


    -- 1. chartsts
    INSERT INTO chartsts_stg (
        svc_ra_cht_nr,
        svc_ra_cht_eff_dt,
        svc_ra_cht_end_dt,
        svc_ra_cht_sts_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT
        svc_ra_cht_nr,
        svc_ra_cht_eff_dt,
        svc_ra_cht_end_dt,
        svc_ra_cht_sts_cd,
        load_ref_te,
        0
    FROM tchart_stg
    UNION
    SELECT DISTINCT 
	    svc_ra_cht_nr,
	    asy_svc_ra_eff_dt, 
	    asy_svc_ra_end_dt, 
	    svc_ra_cht_sts_cd,
        load_ref_te,
        0
    FROM tasyra_stg;

    -- 1b. MERGE chartsts_stg into actual chartsts table
    -- This ensures we get the correct zch_sts_nr values from the actual table
    WITH src_dedup AS (
        SELECT DISTINCT
               svc_ra_cht_nr,
               svc_ra_cht_eff_dt,
               svc_ra_cht_end_dt,
               svc_ra_cht_sts_cd,
               load_ref_te
          FROM chartsts_stg
    ),
    sts_merge AS (
        MERGE INTO chartsts AS tgt
        USING src_dedup AS src
        ON (
            tgt.svc_ra_cht_nr     = src.svc_ra_cht_nr AND
            tgt.svc_ra_cht_eff_dt = src.svc_ra_cht_eff_dt AND
            tgt.svc_ra_cht_end_dt = src.svc_ra_cht_end_dt
        )
        WHEN MATCHED AND (
            tgt.svc_ra_cht_sts_cd IS DISTINCT FROM src.svc_ra_cht_sts_cd OR
            tgt.load_ref_te IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            svc_ra_cht_sts_cd = src.svc_ra_cht_sts_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
               svc_ra_cht_nr,
               svc_ra_cht_eff_dt,
               svc_ra_cht_end_dt,
               svc_ra_cht_sts_cd,
               load_ref_te
            ) VALUES (
               src.svc_ra_cht_nr,
               src.svc_ra_cht_eff_dt,
               src.svc_ra_cht_end_dt,
               src.svc_ra_cht_sts_cd,
               src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO chartsts_merge_actions (action)
    SELECT merge_action FROM sts_merge;

    -- Count merge actions
    SELECT 
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO v_chartsts_ins, v_chartsts_upd, v_chartsts_del
    FROM chartsts_merge_actions;

    -- 2. accrate
    INSERT INTO accrate_stg (
        zch_sts_nr,
        asy_svc_ra,
        del_zn_nr,
        ra_chg_csf_typ_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT
        c.zch_sts_nr,
        t.asy_svc_ra,
        t.del_zn_nr,
        t.ra_chg_csf_typ_cd,
        t.load_ref_te,
        0
    FROM tasyra_stg t
    JOIN chartsts c ON c.svc_ra_cht_nr = t.svc_ra_cht_nr 
        AND c.svc_ra_cht_eff_dt = t.asy_svc_ra_eff_dt
        AND c.svc_ra_cht_end_dt = t.asy_svc_ra_end_dt
        AND c.svc_ra_cht_sts_cd = t.svc_ra_cht_sts_cd;

    GET DIAGNOSTICS v_accrate_ins = ROW_COUNT;

    -- 3. accratecrit
    INSERT INTO accratecrit_stg (
        zch_sts_nr,
        asy_svc_ra,
        dtr_cri_vlu_typ_cd,
        dtr_cri_lo_rng_te,
        dtr_cri_hi_rng_te,
        ccl_mth_typ_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT
        c.zch_sts_nr,
        t.asy_svc_ra,
        t.dtr_cri_vlu_typ_cd,
        t.dtr_cri_lo_rng_te,
        t.dtr_cri_hi_rng_te,
        t.ccl_mth_typ_cd,
        t.load_ref_te,
        0
    FROM tasyra_stg t
    JOIN chartsts c ON c.svc_ra_cht_nr = t.svc_ra_cht_nr 
        AND c.svc_ra_cht_eff_dt = t.asy_svc_ra_eff_dt
        AND c.svc_ra_cht_end_dt = t.asy_svc_ra_end_dt
        AND c.svc_ra_cht_sts_cd = t.svc_ra_cht_sts_cd;

    GET DIAGNOSTICS v_accratecrit_ins = ROW_COUNT;

    -- 4. chartsvcpkg
    INSERT INTO chartsvcpkg_stg (
        zch_sts_nr,
        svc_typ_cd,
        pkg_cha_typ_cd,
        svc_fea_typ_cd,
        pkg_acq_mth_typ_cd,
        na_nrs_cd,
        svc_ra_cht_seq_nr,
        pkg_acq_mth_csf_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT
        c.zch_sts_nr,
        t.svc_typ_cd,
        t.pkg_cha_typ_cd,
        t.svc_fea_typ_cd,
        t.pkg_acq_mth_typ_cd,
        t.na_nrs_cd,
        t.svc_ra_cht_seq_nr,
        t.pkg_acq_mth_csf_cd,
        t.load_ref_te,
        0
    FROM tchart_stg t
    JOIN chartsts c ON c.svc_ra_cht_nr = t.svc_ra_cht_nr 
        AND c.svc_ra_cht_eff_dt = t.svc_ra_cht_eff_dt
        AND c.svc_ra_cht_end_dt = t.svc_ra_cht_end_dt
        AND c.svc_ra_cht_sts_cd = t.svc_ra_cht_sts_cd;

    GET DIAGNOSTICS v_chartsvcpkg_ins = ROW_COUNT;

    -- 5. chartorggeo
    INSERT INTO chartorggeo_stg (
        zch_sts_nr,
        xpt_cny_cd,
        gpu_xpt_cny_cd,
        ipt_cny_cd,
        gpu_ipt_cny_cd,
        svc_typ_cd,
        pkg_cha_typ_cd,
        cus_cls_typ_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT
        c.zch_sts_nr,
        t.xpt_cny_cd,
        t.gpu_xpt_cny_cd,
        t.ipt_cny_cd,
        t.gpu_ipt_cny_cd,
        t.svc_typ_cd,
        t.pkg_cha_typ_cd,
        t.cus_cls_typ_cd,
        t.load_ref_te,
        0
    FROM tchart_stg t
    JOIN chartsts c ON c.svc_ra_cht_nr = t.svc_ra_cht_nr 
        AND c.svc_ra_cht_eff_dt = t.svc_ra_cht_eff_dt
        AND c.svc_ra_cht_end_dt = t.svc_ra_cht_end_dt
        AND c.svc_ra_cht_sts_cd = t.svc_ra_cht_sts_cd;

    GET DIAGNOSTICS v_chartorggeo_ins = ROW_COUNT;

    -- 6. chartacccd
    INSERT INTO chartacccd_stg (
        zch_sts_nr,
        asy_svc_typ_cd,
        bil_ter_typ_cd,
        mvm_drc_cd,
        ccy_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT
        c.zch_sts_nr,
        t.asy_svc_typ_cd,
        t.bil_ter_typ_cd,
        t.mvm_drc_cd,
        t.ccy_cd,
        t.load_ref_te,
        0
    FROM tchart_stg t
    JOIN chartsts c ON c.svc_ra_cht_nr = t.svc_ra_cht_nr 
        AND c.svc_ra_cht_eff_dt = t.svc_ra_cht_eff_dt
        AND c.svc_ra_cht_end_dt = t.svc_ra_cht_end_dt
        AND c.svc_ra_cht_sts_cd = t.svc_ra_cht_sts_cd;

    GET DIAGNOSTICS v_chartacccd_ins = ROW_COUNT;

	-- aggregate inserts across all normalized staging tables; no updates/deletes here
	insertcount := coalesce(v_chartsts_ins, 0)
		+ coalesce(v_accrate_ins, 0)
		+ coalesce(v_accratecrit_ins, 0)
		+ coalesce(v_chartsvcpkg_ins, 0)
		+ coalesce(v_chartorggeo_ins, 0)
		+ coalesce(v_chartacccd_ins, 0);
	updatecount := coalesce(v_chartsts_upd, 0);
	deletecount := coalesce(v_chartsts_del, 0);

    errornumber    := NULL;
    errorstate     := NULL;
    errorprocedure := 'sp_ratechart_accessorialrates_stagingdataset_proc';
    errorline      := NULL;
    errormessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    errornumber    := SQLSTATE;
    errorstate     := SQLSTATE;
    errorprocedure := 'sp_ratechart_accessorialrates_stagingdataset_proc';
    GET STACKED DIAGNOSTICS errorline = PG_EXCEPTION_CONTEXT;
    errormessage   := SQLERRM;
	insertcount := 0;
	updatecount := 0;
	deletecount := 0;
END;
$BODY$;
