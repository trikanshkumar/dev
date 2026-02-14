CREATE OR REPLACE PROCEDURE sp_domesticzone_merge_proc(
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

-- NOTE: DOMZCHARTSTS merge is performed in sp_domesticzone_stagingdataset_proc
-- to get the correct zch_sts_nr identity values for populating other staging tables

-- 1. MERGE domzchartlkup (Domestic Zone Chart Lookup)
WITH src_dedup AS (
    SELECT
           zch_nr,
           zch_sht_dsc_te,
           zch_lg_dsc_te,
           load_ref_te
      FROM domzchartlkup_stg
),
lkup_merge AS (
    MERGE INTO domzchartlkup AS tgt
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
    SELECT 'domzchartlkup', merge_action FROM lkup_merge;

    -- 2. MERGE tdozndt (Domestic Zone Detail - Normalized)
    WITH src_dedup AS (
        SELECT
               zch_sts_nr,
               svc_typ_cd,
               zn_ncv_typ_cd,
               load_ref_te
          FROM tdozndt_new_stg
    ),
    dtl_merge AS (
        MERGE INTO tdozndt AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr AND
            tgt.svc_typ_cd = src.svc_typ_cd
        )
        WHEN MATCHED AND (
            tgt.zn_ncv_typ_cd IS DISTINCT FROM src.zn_ncv_typ_cd OR
            tgt.load_ref_te IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
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
    SELECT 'tdozndt', merge_action FROM dtl_merge;

    -- 3. MERGE tdoznhd (Domestic Zone Header - Normalized)
    -- PK is zch_sts_nr only, so use DISTINCT ON to get one row per zch_sts_nr
    WITH src_dedup AS (
        SELECT 
               zch_sts_nr,
               svc_typ_cd,
               mvm_drc_cd,
               load_ref_te
          FROM tdoznhd_new_stg
    ),
    hdr_merge AS (
        MERGE INTO tdoznhd AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr
        )
        WHEN MATCHED AND (
            tgt.svc_typ_cd IS DISTINCT FROM src.svc_typ_cd OR
            tgt.mvm_drc_cd IS DISTINCT FROM src.mvm_drc_cd OR
            tgt.load_ref_te IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            svc_typ_cd = src.svc_typ_cd,
            mvm_drc_cd = src.mvm_drc_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                svc_typ_cd,
                mvm_drc_cd,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.svc_typ_cd,
                src.mvm_drc_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'tdoznhd', merge_action FROM hdr_merge;

    -- 4. MERGE domzchartdtngeo (Domestic Zone Chart Destination Geo)
    WITH src_dedup AS (
        SELECT
               zch_sts_nr,
               dtn_cny_cd,
               dtn_gpu_nr,
               dtn_rng_lo_psl_cd,
               dtn_rng_hi_psl_cd,
               del_zn_nr,
               load_ref_te
          FROM domzchartdtngeo_stg
    ),
    dtngeo_merge AS (
        MERGE INTO domzchartdtngeo AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr AND
            tgt.dtn_gpu_nr = src.dtn_gpu_nr AND
            tgt.dtn_rng_lo_psl_cd = src.dtn_rng_lo_psl_cd
        )
        WHEN MATCHED AND (
            tgt.dtn_cny_cd IS DISTINCT FROM src.dtn_cny_cd OR
            tgt.dtn_rng_hi_psl_cd IS DISTINCT FROM src.dtn_rng_hi_psl_cd OR
            tgt.del_zn_nr IS DISTINCT FROM src.del_zn_nr OR
            tgt.load_ref_te IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            dtn_cny_cd = src.dtn_cny_cd,
            dtn_rng_hi_psl_cd = src.dtn_rng_hi_psl_cd,
            del_zn_nr = src.del_zn_nr,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_sts_nr,
                dtn_cny_cd,
                dtn_gpu_nr,
                dtn_rng_lo_psl_cd,
                dtn_rng_hi_psl_cd,
                del_zn_nr,
                load_ref_te
            ) VALUES (
                src.zch_sts_nr,
                src.dtn_cny_cd,
                src.dtn_gpu_nr,
                src.dtn_rng_lo_psl_cd,
                src.dtn_rng_hi_psl_cd,
                src.del_zn_nr,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO merge_actions (table_name, action)
    SELECT 'domzchartdtngeo', merge_action FROM dtngeo_merge;

    -- 5. MERGE domzchartorggeo (Domestic Zone Chart Origin Geo)
    WITH src_dedup AS (
        SELECT
               zch_sts_nr,
               org_cny_cd,
               org_gpu_nr,
               org_rng_lo_psl_cd,
               org_rng_hi_psl_cd,
               load_ref_te
          FROM domzchartorggeo_stg
    ),
    orggeo_merge AS (
        MERGE INTO domzchartorggeo AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_sts_nr = src.zch_sts_nr AND
            tgt.org_gpu_nr = src.org_gpu_nr AND
            tgt.org_rng_lo_psl_cd = src.org_rng_lo_psl_cd
        )
        WHEN MATCHED AND (
            tgt.org_cny_cd IS DISTINCT FROM src.org_cny_cd OR
            tgt.org_rng_hi_psl_cd IS DISTINCT FROM src.org_rng_hi_psl_cd OR
            tgt.load_ref_te IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            org_cny_cd = src.org_cny_cd,
            org_rng_hi_psl_cd = src.org_rng_hi_psl_cd,
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
    SELECT 'domzchartorggeo', merge_action FROM orggeo_merge;

    -- Aggregate counts across all tables
    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO insertcount, updatecount, deletecount
    FROM merge_actions;

    errornumber    := NULL;
    errorstate     := NULL;
    errorprocedure := 'sp_domesticzone_merge_proc';
    errorline      := NULL;
    errormessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    errornumber    := SQLSTATE;
    errorstate     := SQLSTATE;
    errorprocedure := 'sp_domesticzone_merge_proc';
    GET STACKED DIAGNOSTICS errorline = PG_EXCEPTION_CONTEXT;
    errormessage   := SQLERRM;
    insertcount := 0;
    updatecount := 0;
    deletecount := 0;
END;
$BODY$;
