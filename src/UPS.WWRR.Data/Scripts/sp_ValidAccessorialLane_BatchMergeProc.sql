CREATE OR REPLACE PROCEDURE sp_validaccessoriallane_batch_merge_proc(
    OUT insertcount integer,
    OUT updatecount integer,
    OUT deletecount integer,
    OUT errornumber text,
    OUT errorstate text,
    OUT errorprocedure text,
    OUT errorline text,
    OUT errormessage text,
    IN batch_size integer DEFAULT 100000)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    v_total_staging bigint := 0;
    v_batch_count integer := 0;
    v_batch_insert integer := 0;
    v_batch_update integer := 0;
    v_batch_delete integer := 0;
    v_batch_matched_no_change integer := 0;
    v_total_batches integer := 0;
    v_offset bigint := 0;
BEGIN
    -- Initialize output counts
    insertcount := 0;
    updatecount := 0;
    deletecount := 0;

    -- Count staging rows
    SELECT COUNT(*) INTO v_total_staging FROM tvasyln_stg;

    -- Handle empty staging
    IF v_total_staging = 0 THEN
        errornumber := NULL; errorstate := NULL;
        errorprocedure := 'sp_validaccessoriallane_batch_merge_proc';
        errorline := NULL; errormessage := NULL;
        RETURN;
    END IF;

    -- Calculate total batches
    v_total_batches := CEIL(v_total_staging::numeric / batch_size);

    -- Create numbered staging table for efficient batching
    DROP TABLE IF EXISTS stg_numbered;
    CREATE TEMP TABLE stg_numbered (
        rn bigint PRIMARY KEY,
        org_cny_cd character varying(5),
        dtn_cny_cd character varying(5),
        asy_svc_typ_cd character varying(5),
        svc_typ_cd character varying(5),
        mvm_drc_cd character varying(5),
        gpn_unt_pir_csf_cd character varying(5),
        apv_sts_cd character varying(5),
        rec_eff_stt_dt date,
        asy_svc_alt_nmc_cd character varying(5),
        svc_typ_alt_nmc_cd character varying(5),
        rec_eff_end_dt date,
        org_gpn_mnm_te character varying(100),
        dtn_gpn_mnm_te character varying(100),
        load_ref_te character varying(255)
    );

    INSERT INTO stg_numbered
    SELECT ROW_NUMBER() OVER () AS rn,
           org_cny_cd, dtn_cny_cd, asy_svc_typ_cd, svc_typ_cd, mvm_drc_cd,
           gpn_unt_pir_csf_cd, apv_sts_cd, rec_eff_stt_dt, asy_svc_alt_nmc_cd,
           svc_typ_alt_nmc_cd, rec_eff_end_dt, org_gpn_mnm_te, dtn_gpn_mnm_te, load_ref_te
    FROM tvasyln_stg;

    -- Create batch temp table
    DROP TABLE IF EXISTS batch_data;
    CREATE TEMP TABLE batch_data (LIKE tvasyln_stg INCLUDING ALL);

    -- Process batches
    WHILE v_offset < v_total_staging LOOP
        v_batch_count := v_batch_count + 1;
        v_batch_insert := 0;
        v_batch_update := 0;
        v_batch_matched_no_change := 0;

        TRUNCATE TABLE batch_data;

        INSERT INTO batch_data (
            org_cny_cd, dtn_cny_cd, asy_svc_typ_cd, svc_typ_cd, mvm_drc_cd,
            gpn_unt_pir_csf_cd, apv_sts_cd, rec_eff_stt_dt, asy_svc_alt_nmc_cd,
            svc_typ_alt_nmc_cd, rec_eff_end_dt, org_gpn_mnm_te, dtn_gpn_mnm_te, load_ref_te
        )
        SELECT 
            org_cny_cd, dtn_cny_cd, asy_svc_typ_cd, svc_typ_cd, mvm_drc_cd,
            gpn_unt_pir_csf_cd, apv_sts_cd, rec_eff_stt_dt, asy_svc_alt_nmc_cd,
            svc_typ_alt_nmc_cd, rec_eff_end_dt, org_gpn_mnm_te, dtn_gpn_mnm_te, load_ref_te
        FROM stg_numbered
        WHERE rn > v_offset AND rn <= v_offset + batch_size;

        WITH merge_result AS (
            MERGE INTO tvasyln AS tgt
            USING batch_data AS src
            ON tgt.org_cny_cd = src.org_cny_cd
               AND tgt.dtn_cny_cd = src.dtn_cny_cd
               AND tgt.asy_svc_typ_cd = src.asy_svc_typ_cd
               AND tgt.svc_typ_cd = src.svc_typ_cd
               AND tgt.mvm_drc_cd = src.mvm_drc_cd
               AND tgt.gpn_unt_pir_csf_cd = src.gpn_unt_pir_csf_cd
               AND tgt.apv_sts_cd = src.apv_sts_cd
               AND tgt.rec_eff_stt_dt = src.rec_eff_stt_dt
               AND tgt.asy_svc_alt_nmc_cd = src.asy_svc_alt_nmc_cd
               AND tgt.org_gpn_mnm_te = src.org_gpn_mnm_te
               AND tgt.dtn_gpn_mnm_te = src.dtn_gpn_mnm_te
            WHEN MATCHED AND (tgt.svc_typ_alt_nmc_cd, tgt.rec_eff_end_dt, tgt.load_ref_te)
                IS DISTINCT FROM (src.svc_typ_alt_nmc_cd, src.rec_eff_end_dt, src.load_ref_te) THEN
                UPDATE SET
                    svc_typ_alt_nmc_cd = src.svc_typ_alt_nmc_cd,
                    rec_eff_end_dt = src.rec_eff_end_dt,
                    load_ref_te = src.load_ref_te
            WHEN MATCHED THEN
                DO NOTHING
            WHEN NOT MATCHED THEN
                INSERT (org_cny_cd, dtn_cny_cd, asy_svc_typ_cd, svc_typ_cd, mvm_drc_cd,
                        gpn_unt_pir_csf_cd, apv_sts_cd, rec_eff_stt_dt, asy_svc_alt_nmc_cd,
                        svc_typ_alt_nmc_cd, rec_eff_end_dt, org_gpn_mnm_te, dtn_gpn_mnm_te, load_ref_te)
                VALUES (src.org_cny_cd, src.dtn_cny_cd, src.asy_svc_typ_cd, src.svc_typ_cd, src.mvm_drc_cd,
                        src.gpn_unt_pir_csf_cd, src.apv_sts_cd, src.rec_eff_stt_dt, src.asy_svc_alt_nmc_cd,
                        src.svc_typ_alt_nmc_cd, src.rec_eff_end_dt, src.org_gpn_mnm_te, src.dtn_gpn_mnm_te, src.load_ref_te)
            RETURNING merge_action()
        )
        SELECT 
            COUNT(*) FILTER (WHERE merge_action = 'INSERT'),
            COUNT(*) FILTER (WHERE merge_action = 'UPDATE'),
            COUNT(*) FILTER (WHERE merge_action = 'DO NOTHING')
        INTO v_batch_insert, v_batch_update, v_batch_matched_no_change
        FROM merge_result;

        insertcount := insertcount + v_batch_insert;
        updatecount := updatecount + v_batch_update;
        v_offset := v_offset + batch_size;
    END LOOP;

    DROP TABLE IF EXISTS batch_data;
    DROP TABLE IF EXISTS stg_numbered;

    -- Batched DELETE for rows in target not in staging
    LOOP
        DELETE FROM tvasyln t
        WHERE t.ctid IN (
            SELECT t2.ctid
            FROM tvasyln t2
            WHERE NOT EXISTS (
                SELECT 1 FROM tvasyln_stg s
                WHERE t2.org_cny_cd = s.org_cny_cd AND t2.dtn_cny_cd = s.dtn_cny_cd
                  AND t2.asy_svc_typ_cd = s.asy_svc_typ_cd AND t2.svc_typ_cd = s.svc_typ_cd
                  AND t2.mvm_drc_cd = s.mvm_drc_cd AND t2.gpn_unt_pir_csf_cd = s.gpn_unt_pir_csf_cd
                  AND t2.apv_sts_cd = s.apv_sts_cd AND t2.rec_eff_stt_dt = s.rec_eff_stt_dt
                  AND t2.asy_svc_alt_nmc_cd = s.asy_svc_alt_nmc_cd
                  AND t2.org_gpn_mnm_te = s.org_gpn_mnm_te AND t2.dtn_gpn_mnm_te = s.dtn_gpn_mnm_te
            )
            LIMIT batch_size
        );

        GET DIAGNOSTICS v_batch_delete = ROW_COUNT;
        deletecount := deletecount + v_batch_delete;

        EXIT WHEN v_batch_delete = 0;
    END LOOP;

    DROP TABLE IF EXISTS batch_data;
    DROP TABLE IF EXISTS stg_numbered;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_validaccessoriallane_batch_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    DROP TABLE IF EXISTS batch_data;
    DROP TABLE IF EXISTS stg_numbered;

    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_validaccessoriallane_batch_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
