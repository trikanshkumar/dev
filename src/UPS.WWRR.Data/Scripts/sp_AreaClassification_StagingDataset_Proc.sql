CREATE OR REPLACE PROCEDURE sp_areaclassification_stagingdataset_proc(
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
	v_zchartsts_ins integer := 0;
	v_zchartsts_upd integer := 0;
	v_zchartsts_del integer := 0;
	v_zchartlkup_ins integer;
	v_tarcldt_ins integer;
	v_tarclhd_ins integer;
	v_zchartdtngeo_ins integer;
	v_zchartdtngpu_ins integer;
	v_zchartorggeo_ins integer;
	v_zchartorggpu_ins integer;
	v_zchartsvctyp_ins integer;
BEGIN
    -- Clear normalized staging tables before populating
    TRUNCATE TABLE zchartsts_stg;
    TRUNCATE TABLE zchartlkup_stg;
    TRUNCATE TABLE tarclhd_new_stg;
	TRUNCATE TABLE tarcldt_new_stg;
    TRUNCATE TABLE zchartdtngeo_stg;
    TRUNCATE TABLE zchartdtngpu_stg;
    TRUNCATE TABLE zchartorggeo_stg;
    TRUNCATE TABLE zchartorggpu_stg;
    TRUNCATE TABLE zchartsvctyp_stg;

    -- Create temp table to track merge actions for zchartsts
    CREATE TEMP TABLE IF NOT EXISTS zchartsts_merge_actions (
        action text
    );
    TRUNCATE TABLE zchartsts_merge_actions;

    -- 1. ZCHARTSTS (Chart Status) - Insert distinct values into staging first
    INSERT INTO zchartsts_stg (
        zch_nr,
        ara_csf_hdr_stt_dt,
        ara_csf_hdr_end_dt,
        bus_eny_acs_sts_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (zch_nr, ara_csf_hdr_stt_dt, ara_csf_hdr_end_dt, bus_eny_acs_sts_cd)
        zch_nr,
        ara_csf_hdr_stt_dt,
        ara_csf_hdr_end_dt,
        bus_eny_acs_sts_cd,
        load_ref_te,
        0
    FROM tarclhd_stg
    ORDER BY zch_nr, ara_csf_hdr_stt_dt, ara_csf_hdr_end_dt, bus_eny_acs_sts_cd;

    -- 1b. MERGE zchartsts_stg into actual zchartsts table
    -- This ensures we get the correct zch_sts_nr values from the actual table
    WITH src_dedup AS (
        SELECT DISTINCT ON (zch_nr, ara_csf_hdr_stt_dt, ara_csf_hdr_end_dt, bus_eny_acs_sts_cd)
               zch_nr,
               ara_csf_hdr_stt_dt,
               ara_csf_hdr_end_dt,
               bus_eny_acs_sts_cd,
               load_ref_te
          FROM zchartsts_stg
         ORDER BY zch_nr, ara_csf_hdr_stt_dt, ara_csf_hdr_end_dt, bus_eny_acs_sts_cd
    ),
    sts_merge AS (
        MERGE INTO zchartsts AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_nr              = src.zch_nr AND
            tgt.ara_csf_hdr_stt_dt  = src.ara_csf_hdr_stt_dt AND
            tgt.ara_csf_hdr_end_dt  = src.ara_csf_hdr_end_dt
        )
        WHEN MATCHED AND (
            tgt.bus_eny_acs_sts_cd IS DISTINCT FROM src.bus_eny_acs_sts_cd
        ) THEN UPDATE SET
            bus_eny_acs_sts_cd = src.bus_eny_acs_sts_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_nr,
                ara_csf_hdr_stt_dt,
                ara_csf_hdr_end_dt,
                bus_eny_acs_sts_cd,
                load_ref_te
            ) VALUES (
                src.zch_nr,
                src.ara_csf_hdr_stt_dt,
                src.ara_csf_hdr_end_dt,
                src.bus_eny_acs_sts_cd,
                src.load_ref_te
            )
        RETURNING merge_action()
    )
    INSERT INTO zchartsts_merge_actions (action)
    SELECT merge_action FROM sts_merge;

    -- Count merge actions
    SELECT 
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO v_zchartsts_ins, v_zchartsts_upd, v_zchartsts_del
    FROM zchartsts_merge_actions;

    -- 2. ZCHARTLKUP (Chart Lookup)
    INSERT INTO zchartlkup_stg (
        zch_nr,
        zch_sht_dsc_te,
        zch_lg_dsc_te,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (zch_nr, zch_sht_dsc_te, zch_lg_dsc_te)
        zch_nr,
        zch_sht_dsc_te,
        zch_lg_dsc_te,
        load_ref_te,
        0
    FROM tarclhd_stg
    ORDER BY zch_nr, zch_sht_dsc_te, zch_lg_dsc_te;

    GET DIAGNOSTICS v_zchartlkup_ins = ROW_COUNT;

    -- 3. TARCLDT (Area Classification Detail) - from tarcldt_stg with join to ACTUAL zchartsts table
    -- Use zch_sts_nr from the actual zchartsts table, not the staging table
    INSERT INTO tarcldt_new_stg (
        zch_sts_nr,
        svc_typ_cd,
        ra_chg_csf_typ_cd,
        ara_csf_dtl_rul_cd,
        ara_csf_dtl_mnt_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (zcs.zch_sts_nr, t.svc_typ_cd, t.ra_chg_csf_typ_cd, t.ara_csf_dtl_rul_cd, t.ara_csf_dtl_mnt_cd)
        zcs.zch_sts_nr,
        t.svc_typ_cd,
        t.ra_chg_csf_typ_cd,
        t.ara_csf_dtl_rul_cd,
        t.ara_csf_dtl_mnt_cd,
        t.load_ref_te,
        0
    FROM tarcldt_stg t
    JOIN zchartsts zcs
        ON zcs.zch_nr = t.zch_nr
        AND zcs.ara_csf_hdr_stt_dt = t.ara_csf_hdr_stt_dt
        AND zcs.ara_csf_hdr_end_dt = t.ara_csf_dtl_end_dt
        AND zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd
    ORDER BY zcs.zch_sts_nr, t.svc_typ_cd, t.ra_chg_csf_typ_cd, t.ara_csf_dtl_rul_cd, t.ara_csf_dtl_mnt_cd;

    GET DIAGNOSTICS v_tarcldt_ins = ROW_COUNT;

    -- 4. TARCLHD (Area Classification Header) - normalized
    -- Use zch_sts_nr from the actual zchartsts table
    INSERT INTO tarclhd_new_stg (
        zch_sts_nr,
        svc_typ_cd,
        asy_svc_typ_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (zcs.zch_sts_nr, t.svc_typ_cd, t.asy_svc_typ_cd)
        zcs.zch_sts_nr,
        t.svc_typ_cd,
        t.asy_svc_typ_cd,
        t.load_ref_te,
        0
    FROM tarclhd_stg t
    JOIN zchartsts zcs
        ON zcs.zch_nr = t.zch_nr
        AND zcs.ara_csf_hdr_stt_dt = t.ara_csf_hdr_stt_dt
        AND zcs.ara_csf_hdr_end_dt = t.ara_csf_hdr_end_dt
        AND zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd
    ORDER BY zcs.zch_sts_nr, t.svc_typ_cd, t.asy_svc_typ_cd;

    GET DIAGNOSTICS v_tarclhd_ins = ROW_COUNT;

    -- 5. ZCHARTDTNGEO (Chart Destination Geo)
    -- Use zch_sts_nr from the actual zchartsts table
    INSERT INTO zchartdtngeo_stg (
        zch_sts_nr,
        dtn_cny_cd,
        dtn_gpu_nr,
        dtn_rng_lo_psl_cd,
        dtn_rng_hi_psl_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (zcs.zch_sts_nr, t.dtn_cny_cd, t.dtn_gpu_nr, t.dtn_rng_lo_psl_cd, t.dtn_rng_hi_psl_cd)
        zcs.zch_sts_nr,
        t.dtn_cny_cd,
        t.dtn_gpu_nr,
        t.dtn_rng_lo_psl_cd,
        t.dtn_rng_hi_psl_cd,
        t.load_ref_te,
        0
    FROM tarcldt_stg t
    JOIN zchartsts zcs
        ON zcs.zch_nr = t.zch_nr
        AND zcs.ara_csf_hdr_stt_dt = t.ara_csf_hdr_stt_dt
        AND zcs.ara_csf_hdr_end_dt = t.ara_csf_dtl_end_dt
        AND zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd
    ORDER BY zcs.zch_sts_nr, t.dtn_cny_cd, t.dtn_gpu_nr, t.dtn_rng_lo_psl_cd, t.dtn_rng_hi_psl_cd;

    GET DIAGNOSTICS v_zchartdtngeo_ins = ROW_COUNT;

    -- 6. ZCHARTDTNGPU (Chart Destination GPU)
    -- Use zch_sts_nr from the actual zchartsts table
    INSERT INTO zchartdtngpu_stg (
        zch_sts_nr,
        dtn_cny_cd,
        dtn_pol_div_2_na,
        dtn_pol_div_1_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (zcs.zch_sts_nr, t.dtn_cny_cd, t.dtn_pol_div_2_na, t.dtn_pol_div_1_cd)
        zcs.zch_sts_nr,
        t.dtn_cny_cd,
        t.dtn_pol_div_2_na,
        t.dtn_pol_div_1_cd,
        t.load_ref_te,
        0
    FROM tarcldt_stg t
    JOIN zchartsts zcs
        ON zcs.zch_nr = t.zch_nr
        AND zcs.ara_csf_hdr_stt_dt = t.ara_csf_hdr_stt_dt
        AND zcs.ara_csf_hdr_end_dt = t.ara_csf_dtl_end_dt
        AND zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd
    ORDER BY zcs.zch_sts_nr, t.dtn_cny_cd, t.dtn_pol_div_2_na, t.dtn_pol_div_1_cd;

    GET DIAGNOSTICS v_zchartdtngpu_ins = ROW_COUNT;

    -- 7. ZCHARTORGGEO (Chart Origin Geo)
    -- Use zch_sts_nr from the actual zchartsts table
    INSERT INTO zchartorggeo_stg (
        zch_sts_nr,
        org_cny_cd,
        org_gpu_nr,
        org_rng_lo_psl_cd,
        org_rng_hi_psl_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (zcs.zch_sts_nr, t.org_cny_cd, t.org_gpu_nr, t.org_rng_lo_psl_cd, t.org_rng_hi_psl_cd)
        zcs.zch_sts_nr,
        t.org_cny_cd,
        t.org_gpu_nr,
        t.org_rng_lo_psl_cd,
        t.org_rng_hi_psl_cd,
        t.load_ref_te,
        0
    FROM tarcldt_stg t
    JOIN zchartsts zcs
        ON zcs.zch_nr = t.zch_nr
        AND zcs.ara_csf_hdr_stt_dt = t.ara_csf_hdr_stt_dt
        AND zcs.ara_csf_hdr_end_dt = t.ara_csf_dtl_end_dt
        AND zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd
    ORDER BY zcs.zch_sts_nr, t.org_cny_cd, t.org_gpu_nr, t.org_rng_lo_psl_cd, t.org_rng_hi_psl_cd;

    GET DIAGNOSTICS v_zchartorggeo_ins = ROW_COUNT;

    -- 8. ZCHARTORGGPU (Chart Origin GPU)
    -- Use zch_sts_nr from the actual zchartsts table
    INSERT INTO zchartorggpu_stg (
        zch_sts_nr,
        org_cny_cd,
        org_pol_div_2_na,
        org_pol_div_1_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (zcs.zch_sts_nr, t.org_cny_cd, t.org_pol_div_2_na, t.org_pol_div_1_cd)
        zcs.zch_sts_nr,
        t.org_cny_cd,
        t.org_pol_div_2_na,
        t.org_pol_div_1_cd,
        t.load_ref_te,
        0
    FROM tarcldt_stg t
    JOIN zchartsts zcs
        ON zcs.zch_nr = t.zch_nr
        AND zcs.ara_csf_hdr_stt_dt = t.ara_csf_hdr_stt_dt
        AND zcs.ara_csf_hdr_end_dt = t.ara_csf_dtl_end_dt
        AND zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd
    ORDER BY zcs.zch_sts_nr, t.org_cny_cd, t.org_pol_div_2_na, t.org_pol_div_1_cd;

    GET DIAGNOSTICS v_zchartorggpu_ins = ROW_COUNT;

    -- 9. ZCHARTSVCTYP (Chart Service Type) - UNION of origin and destination
    INSERT INTO zchartsvctyp_stg (
        cny_cd,
        gpu_nr,
        svc_typ_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (cny_cd, gpu_nr, svc_typ_cd)
        cny_cd,
        gpu_nr,
        svc_typ_cd,
        load_ref_te,
        0
    FROM (
        SELECT
            org_cny_cd AS cny_cd,
            org_gpu_nr AS gpu_nr,
            svc_typ_cd,
            load_ref_te
        FROM tarcldt_stg
        UNION
        SELECT
            dtn_cny_cd,
            dtn_gpu_nr,
            svc_typ_cd,
            load_ref_te
        FROM tarcldt_stg
    ) sub
    ORDER BY cny_cd, gpu_nr, svc_typ_cd;

    GET DIAGNOSTICS v_zchartsvctyp_ins = ROW_COUNT;

	-- aggregate inserts across all normalized staging tables
	insertcount := coalesce(v_zchartsts_ins, 0)
		+ coalesce(v_zchartlkup_ins, 0)
		+ coalesce(v_tarcldt_ins, 0)
		+ coalesce(v_tarclhd_ins, 0)
		+ coalesce(v_zchartdtngeo_ins, 0)
		+ coalesce(v_zchartdtngpu_ins, 0)
		+ coalesce(v_zchartorggeo_ins, 0)
		+ coalesce(v_zchartorggpu_ins, 0)
		+ coalesce(v_zchartsvctyp_ins, 0);
	updatecount := coalesce(v_zchartsts_upd, 0);
	deletecount := coalesce(v_zchartsts_del, 0);

    errornumber    := NULL;
    errorstate     := NULL;
    errorprocedure := 'sp_areaclassification_stagingdataset_proc';
    errorline      := NULL;
    errormessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    errornumber    := SQLSTATE;
    errorstate     := SQLSTATE;
    errorprocedure := 'sp_areaclassification_stagingdataset_proc';
    GET STACKED DIAGNOSTICS errorline = PG_EXCEPTION_CONTEXT;
    errormessage   := SQLERRM;
	insertcount := 0;
	updatecount := 0;
	deletecount := 0;
END;
$BODY$;
