CREATE OR REPLACE PROCEDURE sp_internationalzone_merge_proc(
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
    CREATE TEMP TABLE IF NOT EXISTS merge_actions (
        table_name text,
        action     text
    );
    TRUNCATE TABLE merge_actions;

    -- NOTE: izchartsts merge is handled in sp_internationalzone_stagingdataset_proc
    -- to ensure correct zch_sts_nr values are used for all dependent staging tables

    -- 1. MERGE izchartlkup (Chart Lookup)
    WITH src_dedup AS (
        SELECT DISTINCT
               zch_nr,
               zch_sht_dsc_te,
               zch_lg_dsc_te,
               load_ref_te
          FROM izchartlkup_stg
    ),
    lkup_merge AS (
        MERGE INTO izchartlkup AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_nr = src.zch_nr
        )
        WHEN MATCHED AND (
            tgt.zch_sht_dsc_te IS DISTINCT FROM src.zch_sht_dsc_te OR
            tgt.zch_lg_dsc_te IS DISTINCT FROM src.zch_lg_dsc_te OR
            tgt.load_ref_te IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            zch_sht_dsc_te = src.zch_sht_dsc_te,
            zch_lg_dsc_te = src.zch_lg_dsc_te,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_nr,
                zch_sht_dsc_te,
                zch_lg_dsc_te,
                load_ref_te
            ) VALUES (
                src.zch_nr,
                src.zch_sht_dsc_te,
                src.zch_lg_dsc_te,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'izchartlkup', merge_action FROM lkup_merge;

    -- 2. MERGE izcharthd (International Zone Chart Header)
    WITH src_dedup AS (
        SELECT DISTINCT
               zch_sts_nr,
               svc_typ_cd,
               mvm_drc_cd,
               pkg_cha_typ_cd,
               load_ref_te
          FROM izcharthd_stg
    ),
    hd_merge AS (
        MERGE INTO izcharthd AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr
        )
        WHEN MATCHED AND (
            tgt.svc_typ_cd IS DISTINCT FROM src.svc_typ_cd OR
            tgt.mvm_drc_cd IS DISTINCT FROM src.mvm_drc_cd OR
            tgt.pkg_cha_typ_cd IS DISTINCT FROM src.pkg_cha_typ_cd OR
            tgt.load_ref_te IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            svc_typ_cd = src.svc_typ_cd,
            mvm_drc_cd = src.mvm_drc_cd,
            pkg_cha_typ_cd = src.pkg_cha_typ_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                svc_typ_cd,
                mvm_drc_cd,
                pkg_cha_typ_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.svc_typ_cd,
                src.mvm_drc_cd,
                src.pkg_cha_typ_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'izcharthd', merge_action FROM hd_merge;

    -- 3. MERGE izchartdtl (International Zone Chart Detail)
    WITH src_dedup AS (
        SELECT DISTINCT
               zch_sts_nr,
               svc_typ_cd,
               zn_ncv_typ_cd,
               load_ref_te
          FROM izchartdtl_stg
    ),
    dtl_merge AS (
        MERGE INTO izchartdtl AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr
        )
        WHEN MATCHED AND (
            tgt.svc_typ_cd IS DISTINCT FROM src.svc_typ_cd OR
            tgt.zn_ncv_typ_cd IS DISTINCT FROM src.zn_ncv_typ_cd OR
            tgt.load_ref_te IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            svc_typ_cd = src.svc_typ_cd,
            zn_ncv_typ_cd = src.zn_ncv_typ_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                svc_typ_cd,
                zn_ncv_typ_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.svc_typ_cd,
                src.zn_ncv_typ_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'izchartdtl', merge_action FROM dtl_merge;

    -- 4. MERGE izchartorgdtnpst (International Zone Chart Origin Destination Postal)
    WITH src_dedup AS (
        SELECT DISTINCT
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
               load_ref_te
          FROM izchartorgdtnpst_stg
    ),
    orgdtnpst_merge AS (
        MERGE INTO izchartorgdtnpst AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr AND
            tgt.org_gpu_nr = src.org_gpu_nr AND
            tgt.org_rng_lo_psl_cd = src.org_rng_lo_psl_cd AND
            tgt.org_rng_hi_psl_cd = src.org_rng_hi_psl_cd AND
            tgt.org_pol_div_2_na = src.org_pol_div_2_na AND
            tgt.dtn_gpu_nr = src.dtn_gpu_nr AND
            tgt.dtn_rng_lo_psl_cd = src.dtn_rng_lo_psl_cd AND
            tgt.dtn_rng_hi_psl_cd = src.dtn_rng_hi_psl_cd AND
            tgt.dtn_pol_div_2_na = src.dtn_pol_div_2_na AND
            tgt.del_zn_nr = src.del_zn_nr
        )
        WHEN MATCHED AND (
            tgt.org_cny_cd IS DISTINCT FROM src.org_cny_cd OR
            tgt.dtn_cny_cd IS DISTINCT FROM src.dtn_cny_cd OR
            tgt.load_ref_te IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            org_cny_cd = src.org_cny_cd,
            dtn_cny_cd = src.dtn_cny_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
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
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.org_cny_cd,
                src.org_rng_lo_psl_cd,
                src.org_rng_hi_psl_cd,
                src.org_gpu_nr,
                src.org_pol_div_2_na,
                src.dtn_cny_cd,
                src.dtn_rng_lo_psl_cd,
                src.dtn_rng_hi_psl_cd,
                src.dtn_gpu_nr,
                src.dtn_pol_div_2_na,
                src.del_zn_nr,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'izchartorgdtnpst', merge_action FROM orgdtnpst_merge;

    -- 5. MERGE izchartorgpoldiv (International Zone Chart Origin Political Division)
    WITH src_dedup AS (
        SELECT DISTINCT
               zch_sts_nr,
               org_cny_cd,
               org_pol_div_1_cd,
               load_ref_te
          FROM izchartorgpoldiv_stg
    ),
    orgpoldiv_merge AS (
        MERGE INTO izchartorgpoldiv AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr AND
            tgt.org_cny_cd = src.org_cny_cd
        )
        WHEN MATCHED AND (
            tgt.org_pol_div_1_cd IS DISTINCT FROM src.org_pol_div_1_cd OR
            tgt.load_ref_te IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            org_pol_div_1_cd = src.org_pol_div_1_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                org_cny_cd,
                org_pol_div_1_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.org_cny_cd,
                src.org_pol_div_1_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'izchartorgpoldiv', merge_action FROM orgpoldiv_merge;

    -- 6. MERGE izchartdtnpoldiv (International Zone Chart Destination Political Division)
    WITH src_dedup AS (
        SELECT DISTINCT
               zch_sts_nr,
               dtn_cny_cd,
               dtn_pol_div_1_cd,
               load_ref_te
          FROM izchartdtnpoldiv_stg
    ),
    dtnpoldiv_merge AS (
        MERGE INTO izchartdtnpoldiv AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr AND
            tgt.dtn_cny_cd = src.dtn_cny_cd
        )
        WHEN MATCHED AND (
            tgt.dtn_pol_div_1_cd IS DISTINCT FROM src.dtn_pol_div_1_cd OR
            tgt.load_ref_te IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            dtn_pol_div_1_cd = src.dtn_pol_div_1_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                dtn_cny_cd,
                dtn_pol_div_1_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.dtn_cny_cd,
                src.dtn_pol_div_1_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'izchartdtnpoldiv', merge_action FROM dtnpoldiv_merge;

    -- Aggregate counts across all tables
    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO insertcount, updatecount, deletecount
    FROM merge_actions;

    errornumber    := NULL;
    errorstate     := NULL;
    errorprocedure := 'sp_internationalzone_merge_proc';
    errorline      := NULL;
    errormessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    errornumber    := SQLSTATE;
    errorstate     := SQLSTATE;
    errorprocedure := 'sp_internationalzone_merge_proc';
    GET STACKED DIAGNOSTICS errorline = PG_EXCEPTION_CONTEXT;
    errormessage   := SQLERRM;
    insertcount := 0;
    updatecount := 0;
    deletecount := 0;
END;
$BODY$;
