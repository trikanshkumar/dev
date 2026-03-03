CREATE OR REPLACE PROCEDURE sp_internationalzone_stagingdataset_proc(
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
	v_izchartsts_ins integer := 0;
	v_izchartsts_upd integer := 0;
	v_izchartsts_del integer := 0;
	v_izchartlkup_ins integer;
	v_izcharthd_ins integer;
	v_izchartdtl_ins integer;
	v_izchartorgdtnpst_ins integer;
	v_izchartorgpoldiv_ins integer;
	v_izchartdtnpoldiv_ins integer;
BEGIN
    -- Clear normalized staging tables before populating
    TRUNCATE TABLE izchartsts_stg;
    TRUNCATE TABLE izchartlkup_stg;
    TRUNCATE TABLE izcharthd_stg;
    TRUNCATE TABLE izchartdtl_stg;
    TRUNCATE TABLE izchartorgdtnpst_stg;
    TRUNCATE TABLE izchartorgpoldiv_stg;
    TRUNCATE TABLE izchartdtnpoldiv_stg;

    -- Create temp table to track merge actions for izchartsts
    CREATE TEMP TABLE IF NOT EXISTS izchartsts_merge_actions (
        action text
    );
    TRUNCATE TABLE izchartsts_merge_actions;

    -- 1. IZCHARTSTS (Chart Status) - Insert distinct values into staging first
    INSERT INTO izchartsts_stg (
        zch_nr,
        inl_zn_hdr_stt_dt,
        inl_zn_hdr_end_dt,
        bus_eny_acs_sts_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (zch_nr, inl_zn_hdr_stt_dt, inl_zn_hdr_end_dt, bus_eny_acs_sts_cd)
        zch_nr,
        inl_zn_hdr_stt_dt,
        inl_zn_hdr_end_dt,
        bus_eny_acs_sts_cd,
        load_ref_te,
        0
    FROM tinznhd_stg
    ORDER BY zch_nr, inl_zn_hdr_stt_dt, inl_zn_hdr_end_dt, bus_eny_acs_sts_cd;

    -- 1b. MERGE izchartsts_stg into actual izchartsts table
    -- This ensures we get the correct zch_sts_nr values from the actual table
    WITH src_dedup AS (
        SELECT DISTINCT ON (zch_nr, inl_zn_hdr_stt_dt, inl_zn_hdr_end_dt, bus_eny_acs_sts_cd)
               zch_nr,
               inl_zn_hdr_stt_dt,
               inl_zn_hdr_end_dt,
               bus_eny_acs_sts_cd,
               load_ref_te
          FROM izchartsts_stg
         ORDER BY zch_nr, inl_zn_hdr_stt_dt, inl_zn_hdr_end_dt, bus_eny_acs_sts_cd
    ),
    sts_merge AS (
        MERGE INTO izchartsts AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_nr              = src.zch_nr AND
            tgt.inl_zn_hdr_stt_dt   = src.inl_zn_hdr_stt_dt AND
            tgt.inl_zn_hdr_end_dt   = src.inl_zn_hdr_end_dt
        )
        WHEN MATCHED AND (
            tgt.bus_eny_acs_sts_cd IS DISTINCT FROM src.bus_eny_acs_sts_cd OR
            tgt.load_ref_te IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            bus_eny_acs_sts_cd = src.bus_eny_acs_sts_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_nr,
                inl_zn_hdr_stt_dt,
                inl_zn_hdr_end_dt,
                bus_eny_acs_sts_cd,
                load_ref_te
            ) VALUES (
                src.zch_nr,
                src.inl_zn_hdr_stt_dt,
                src.inl_zn_hdr_end_dt,
                src.bus_eny_acs_sts_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO izchartsts_merge_actions (action)
    SELECT merge_action FROM sts_merge;

    -- Count merge actions
    SELECT 
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO v_izchartsts_ins, v_izchartsts_upd, v_izchartsts_del
    FROM izchartsts_merge_actions;

    -- 2. IZCHARTLKUP (Chart Lookup)
    INSERT INTO izchartlkup_stg (
        zch_nr,
        zch_sht_dsc_te,
        zch_lg_dsc_te,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT
        t.zch_nr,
        MIN(t.zch_sht_dsc_te),
        MAX(t.zch_lg_dsc_te) ||
          ' | Original short descriptions: ' ||
          STRING_AGG(DISTINCT t.zch_sht_dsc_te, ' ; ' ORDER BY t.zch_sht_dsc_te),
        MAX(t.load_ref_te),
        0
    FROM tinznhd_stg t
    GROUP BY t.zch_nr;

    GET DIAGNOSTICS v_izchartlkup_ins = ROW_COUNT;

    -- 3. IZCHARTHD (International Zone Chart Header)
    -- Use zch_sts_nr from the actual izchartsts table
    INSERT INTO izcharthd_stg (
        zch_sts_nr,
        svc_typ_cd,
        mvm_drc_cd,
        pkg_cha_typ_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (zcs.zch_sts_nr, t.svc_typ_cd, t.mvm_drc_cd, t.pkg_cha_typ_cd)
        zcs.zch_sts_nr,
        t.svc_typ_cd,
        t.mvm_drc_cd,
        t.pkg_cha_typ_cd,
        t.load_ref_te,
        0
    FROM tinznhd_stg t
    JOIN izchartsts zcs
        ON zcs.zch_nr = t.zch_nr
        AND zcs.inl_zn_hdr_stt_dt = t.inl_zn_hdr_stt_dt
        AND zcs.inl_zn_hdr_end_dt = t.inl_zn_hdr_end_dt
        AND zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd
    ORDER BY zcs.zch_sts_nr, t.svc_typ_cd, t.mvm_drc_cd, t.pkg_cha_typ_cd;

    GET DIAGNOSTICS v_izcharthd_ins = ROW_COUNT;

    -- 4. IZCHARTDTL (International Zone Chart Detail)
    -- Use zch_sts_nr from the actual izchartsts table
    INSERT INTO izchartdtl_stg (
        zch_sts_nr,
        svc_typ_cd,
        zn_ncv_typ_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (zcs.zch_sts_nr, t.svc_typ_cd, t.zn_ncv_typ_cd)
        zcs.zch_sts_nr,
        t.svc_typ_cd,
        t.zn_ncv_typ_cd,
        t.load_ref_te,
        0
    FROM tinzndt_stg t
    JOIN izchartsts zcs
        ON zcs.zch_nr = t.zch_nr
        AND zcs.inl_zn_hdr_stt_dt = t.inl_zn_hdr_stt_dt
        AND zcs.inl_zn_hdr_end_dt = t.inl_zn_dtl_end_dt
        AND zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd
    ORDER BY zcs.zch_sts_nr, t.svc_typ_cd, t.zn_ncv_typ_cd;

    GET DIAGNOSTICS v_izchartdtl_ins = ROW_COUNT;

    -- 5. IZCHARTORGDTNPST (International Zone Chart Origin Destination Postal)
    -- Use zch_sts_nr from the actual izchartsts table
    INSERT INTO izchartorgdtnpst_stg (
        zch_sts_nr,
        org_cny_cd,
        org_rng_lo_psl_cd,
        org_rng_hi_psl_cd,
        org_gpu_nr,
        org_pol_div_2_na,
        dtn_cny_cd,
        dtn_rng_lo_psl_cd,
        dtn_rng_hi_psl_cd,
        dtn_gpu_nr,
        dtn_pol_div_2_na,
        del_zn_nr,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (zcs.zch_sts_nr, t.org_cny_cd, t.org_rng_lo_psl_cd, t.org_rng_hi_psl_cd, t.org_gpu_nr, t.org_pol_div_2_na, t.dtn_cny_cd, t.dtn_rng_lo_psl_cd, t.dtn_rng_hi_psl_cd, t.dtn_gpu_nr, t.dtn_pol_div_2_na, t.del_zn_nr)
        zcs.zch_sts_nr,
        t.org_cny_cd,
        t.org_rng_lo_psl_cd,
        t.org_rng_hi_psl_cd,
        t.org_gpu_nr,
        t.org_pol_div_2_na,
        t.dtn_cny_cd,
        t.dtn_rng_lo_psl_cd,
        t.dtn_rng_hi_psl_cd,
        t.dtn_gpu_nr,
        t.dtn_pol_div_2_na,
        t.del_zn_nr,
        t.load_ref_te,
        0
    FROM tinzndt_stg t
    JOIN izchartsts zcs
        ON zcs.zch_nr = t.zch_nr
        AND zcs.inl_zn_hdr_stt_dt = t.inl_zn_hdr_stt_dt
        AND zcs.inl_zn_hdr_end_dt = t.inl_zn_dtl_end_dt
        AND zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd
    ORDER BY zcs.zch_sts_nr, t.org_cny_cd, t.org_rng_lo_psl_cd, t.org_rng_hi_psl_cd, t.org_gpu_nr, t.org_pol_div_2_na, t.dtn_cny_cd, t.dtn_rng_lo_psl_cd, t.dtn_rng_hi_psl_cd, t.dtn_gpu_nr, t.dtn_pol_div_2_na, t.del_zn_nr;

    GET DIAGNOSTICS v_izchartorgdtnpst_ins = ROW_COUNT;

    -- 6. IZCHARTORGPOLDIV (International Zone Chart Origin Political Division)
    -- Use zch_sts_nr from the actual izchartsts table
    INSERT INTO izchartorgpoldiv_stg (
        zch_sts_nr,
        org_cny_cd,
        org_pol_div_1_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (zcs.zch_sts_nr, t.org_cny_cd, t.org_pol_div_1_cd)
        zcs.zch_sts_nr,
        t.org_cny_cd,
        t.org_pol_div_1_cd,
        t.load_ref_te,
        0
    FROM tinzndt_stg t
    JOIN izchartsts zcs
        ON zcs.zch_nr = t.zch_nr
        AND zcs.inl_zn_hdr_stt_dt = t.inl_zn_hdr_stt_dt
        AND zcs.inl_zn_hdr_end_dt = t.inl_zn_dtl_end_dt
        AND zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd
    ORDER BY zcs.zch_sts_nr, t.org_cny_cd, t.org_pol_div_1_cd;

    GET DIAGNOSTICS v_izchartorgpoldiv_ins = ROW_COUNT;

    -- 7. IZCHARTDTNPOLDIV (International Zone Chart Destination Political Division)
    -- Use zch_sts_nr from the actual izchartsts table
    INSERT INTO izchartdtnpoldiv_stg (
        zch_sts_nr,
        dtn_cny_cd,
        dtn_pol_div_1_cd,
        load_ref_te,
        is_completed_ir
    )
    SELECT DISTINCT ON (zcs.zch_sts_nr, t.dtn_cny_cd, t.dtn_pol_div_1_cd)
        zcs.zch_sts_nr,
        t.dtn_cny_cd,
        t.dtn_pol_div_1_cd,
        t.load_ref_te,
        0
    FROM tinzndt_stg t
    JOIN izchartsts zcs
        ON zcs.zch_nr = t.zch_nr
        AND zcs.inl_zn_hdr_stt_dt = t.inl_zn_hdr_stt_dt
        AND zcs.inl_zn_hdr_end_dt = t.inl_zn_dtl_end_dt
        AND zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd
    ORDER BY zcs.zch_sts_nr, t.dtn_cny_cd, t.dtn_pol_div_1_cd;

    GET DIAGNOSTICS v_izchartdtnpoldiv_ins = ROW_COUNT;

    -- Aggregate inserts across all normalized staging tables
    insertcount := COALESCE(v_izchartsts_ins, 0)
        + COALESCE(v_izchartlkup_ins, 0)
        + COALESCE(v_izcharthd_ins, 0)
        + COALESCE(v_izchartdtl_ins, 0)
        + COALESCE(v_izchartorgdtnpst_ins, 0)
        + COALESCE(v_izchartorgpoldiv_ins, 0)
        + COALESCE(v_izchartdtnpoldiv_ins, 0);
    updatecount := COALESCE(v_izchartsts_upd, 0);
    deletecount := COALESCE(v_izchartsts_del, 0);

    errornumber    := NULL;
    errorstate     := NULL;
    errorprocedure := 'sp_internationalzone_stagingdataset_proc';
    errorline      := NULL;
    errormessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    errornumber    := SQLSTATE;
    errorstate     := SQLSTATE;
    errorprocedure := 'sp_internationalzone_stagingdataset_proc';
    GET STACKED DIAGNOSTICS errorline = PG_EXCEPTION_CONTEXT;
    errormessage   := SQLERRM;
    insertcount := 0;
    updatecount := 0;
    deletecount := 0;
END;
$BODY$;
