CREATE OR REPLACE PROCEDURE sp_areaclassification_merge_proc(
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

    -- NOTE: zchartsts merge is handled in sp_areaclassification_stagingdataset_proc
    -- to ensure correct zch_sts_nr values are used for all dependent staging tables

    -- 1. MERGE zchartlkup (Chart Lookup)
    WITH src_dedup AS (
        SELECT zch_nr,
               zch_sht_dsc_te,
               zch_lg_dsc_te,
               load_ref_te
          FROM zchartlkup_stg
    ),
    lkup_merge AS (
        MERGE INTO zchartlkup AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_nr = src.zch_nr
        )
        WHEN MATCHED AND (
            tgt.zch_sht_dsc_te IS DISTINCT FROM src.zch_sht_dsc_te OR
            tgt.zch_lg_dsc_te IS DISTINCT FROM src.zch_lg_dsc_te
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
    SELECT 'zchartlkup', merge_action FROM lkup_merge;

    -- 2. MERGE tarcldt (Area Classification Detail - Normalized)
    WITH src_dedup AS (
        SELECT zch_sts_nr,
               svc_typ_cd,
               ra_chg_csf_typ_cd,
               ara_csf_dtl_rul_cd,
               ara_csf_dtl_mnt_cd,
               load_ref_te
          FROM tarcldt_new_stg
    ),
    dtl_merge AS (
        MERGE INTO tarcldt AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr AND
            tgt.svc_typ_cd = src.svc_typ_cd AND
            tgt.ra_chg_csf_typ_cd = src.ra_chg_csf_typ_cd
        )
        WHEN MATCHED AND (
            tgt.ara_csf_dtl_rul_cd IS DISTINCT FROM src.ara_csf_dtl_rul_cd OR
            tgt.ara_csf_dtl_mnt_cd IS DISTINCT FROM src.ara_csf_dtl_mnt_cd
        ) THEN UPDATE SET
            ara_csf_dtl_rul_cd = src.ara_csf_dtl_rul_cd,
            ara_csf_dtl_mnt_cd = src.ara_csf_dtl_mnt_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                svc_typ_cd,
                ra_chg_csf_typ_cd,
                ara_csf_dtl_rul_cd,
                ara_csf_dtl_mnt_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.svc_typ_cd,
                src.ra_chg_csf_typ_cd,
                src.ara_csf_dtl_rul_cd,
                src.ara_csf_dtl_mnt_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'tarcldt', merge_action FROM dtl_merge;

    -- 3. MERGE tarclhd (Area Classification Header - Normalized)
    WITH src_dedup AS (
        SELECT zch_sts_nr,
               svc_typ_cd,
               asy_svc_typ_cd,
               load_ref_te
          FROM tarclhd_new_stg
    ),
    hdr_merge AS (
        MERGE INTO tarclhd AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr
        )
        WHEN MATCHED AND (
            tgt.svc_typ_cd IS DISTINCT FROM src.svc_typ_cd OR
            tgt.asy_svc_typ_cd IS DISTINCT FROM src.asy_svc_typ_cd
        ) THEN UPDATE SET
            svc_typ_cd = src.svc_typ_cd,
            asy_svc_typ_cd = src.asy_svc_typ_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                svc_typ_cd,
                asy_svc_typ_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.svc_typ_cd,
                src.asy_svc_typ_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'tarclhd', merge_action FROM hdr_merge;

    -- 4. MERGE zchartdtngeo (Chart Destination Geo)
    WITH src_dedup AS (
        SELECT zch_sts_nr,
               dtn_cny_cd,
               dtn_gpu_nr,
               dtn_rng_lo_psl_cd,
               dtn_rng_hi_psl_cd,
               load_ref_te
          FROM zchartdtngeo_stg
    ),
    dtngeo_merge AS (
        MERGE INTO zchartdtngeo AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr AND
            tgt.dtn_gpu_nr = src.dtn_gpu_nr AND
            tgt.dtn_rng_lo_psl_cd = src.dtn_rng_lo_psl_cd AND
            tgt.dtn_rng_hi_psl_cd = src.dtn_rng_hi_psl_cd
        )
        WHEN MATCHED AND (
            tgt.dtn_cny_cd IS DISTINCT FROM src.dtn_cny_cd
        ) THEN UPDATE SET
            dtn_cny_cd = src.dtn_cny_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                dtn_cny_cd,
                dtn_gpu_nr,
                dtn_rng_lo_psl_cd,
                dtn_rng_hi_psl_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.dtn_cny_cd,
                src.dtn_gpu_nr,
                src.dtn_rng_lo_psl_cd,
                src.dtn_rng_hi_psl_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'zchartdtngeo', merge_action FROM dtngeo_merge;

    -- 5. MERGE zchartdtngpu (Chart Destination GPU)
    WITH src_dedup AS (
        SELECT zch_sts_nr,
               dtn_cny_cd,
               dtn_pol_div_2_na,
               dtn_pol_div_1_cd,
               load_ref_te
          FROM zchartdtngpu_stg
    ),
    dtngpu_merge AS (
        MERGE INTO zchartdtngpu AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr AND
            tgt.dtn_cny_cd = src.dtn_cny_cd AND
            tgt.dtn_pol_div_2_na = src.dtn_pol_div_2_na
        )
        WHEN MATCHED AND (
            tgt.dtn_pol_div_1_cd IS DISTINCT FROM src.dtn_pol_div_1_cd
        ) THEN UPDATE SET
            dtn_pol_div_1_cd = src.dtn_pol_div_1_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                dtn_cny_cd,
                dtn_pol_div_2_na,
                dtn_pol_div_1_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.dtn_cny_cd,
                src.dtn_pol_div_2_na,
                src.dtn_pol_div_1_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'zchartdtngpu', merge_action FROM dtngpu_merge;

    -- 6. MERGE zchartorggeo (Chart Origin Geo)
    WITH src_dedup AS (
        SELECT zch_sts_nr,
               org_cny_cd,
               org_gpu_nr,
               org_rng_lo_psl_cd,
               org_rng_hi_psl_cd,
               load_ref_te
          FROM zchartorggeo_stg
    ),
    orggeo_merge AS (
        MERGE INTO zchartorggeo AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr AND
            tgt.org_gpu_nr = src.org_gpu_nr AND
            tgt.org_rng_lo_psl_cd = src.org_rng_lo_psl_cd AND
            tgt.org_rng_hi_psl_cd = src.org_rng_hi_psl_cd
        )
        WHEN MATCHED AND (
            tgt.org_cny_cd IS DISTINCT FROM src.org_cny_cd
        ) THEN UPDATE SET
            org_cny_cd = src.org_cny_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                org_cny_cd,
                org_gpu_nr,
                org_rng_lo_psl_cd,
                org_rng_hi_psl_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.org_cny_cd,
                src.org_gpu_nr,
                src.org_rng_lo_psl_cd,
                src.org_rng_hi_psl_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'zchartorggeo', merge_action FROM orggeo_merge;

    -- 7. MERGE zchartorggpu (Chart Origin GPU)
    WITH src_dedup AS (
        SELECT zch_sts_nr,
               org_cny_cd,
               org_pol_div_2_na,
               org_pol_div_1_cd,
               load_ref_te
          FROM zchartorggpu_stg
    ),
    orggpu_merge AS (
        MERGE INTO zchartorggpu AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr AND
            tgt.org_cny_cd = src.org_cny_cd AND
            tgt.org_pol_div_2_na = src.org_pol_div_2_na
        )
        WHEN MATCHED AND (
            tgt.org_pol_div_1_cd IS DISTINCT FROM src.org_pol_div_1_cd
        ) THEN UPDATE SET
            org_pol_div_1_cd = src.org_pol_div_1_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                org_cny_cd,
                org_pol_div_2_na,
                org_pol_div_1_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.org_cny_cd,
                src.org_pol_div_2_na,
                src.org_pol_div_1_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'zchartorggpu', merge_action FROM orggpu_merge;

    -- 8. MERGE zchartsvctyp (Chart Service Type)
    WITH src_dedup AS (
        SELECT cny_cd,
               gpu_nr,
               svc_typ_cd,
               load_ref_te
          FROM zchartsvctyp_stg
    ),
    svctyp_merge AS (
        MERGE INTO zchartsvctyp AS tgt
        USING src_dedup AS src
        ON (
            tgt.cny_cd = src.cny_cd AND
            tgt.gpu_nr = src.gpu_nr AND
            tgt.svc_typ_cd = src.svc_typ_cd
        )
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                cny_cd,
                gpu_nr,
                svc_typ_cd,
                load_ref_te
            ) VALUES (
                src.cny_cd,
                src.gpu_nr,
                src.svc_typ_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'zchartsvctyp', merge_action FROM svctyp_merge;

    -- Aggregate counts across all tables
    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO insertcount, updatecount, deletecount
    FROM merge_actions;

    errornumber    := NULL;
    errorstate     := NULL;
    errorprocedure := 'sp_areaclassification_merge_proc';
    errorline      := NULL;
    errormessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    errornumber    := SQLSTATE;
    errorstate     := SQLSTATE;
    errorprocedure := 'sp_areaclassification_merge_proc';
    GET STACKED DIAGNOSTICS errorline = PG_EXCEPTION_CONTEXT;
    errormessage   := SQLERRM;
    insertcount := 0;
    updatecount := 0;
    deletecount := 0;
END;
$BODY$;
