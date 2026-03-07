CREATE OR REPLACE PROCEDURE sp_fuelsurcharge_batch_merge_proc(
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
    SELECT COUNT(*) INTO v_total_staging FROM tsubchg_stg;

    -- Handle empty staging
    IF v_total_staging = 0 THEN
        errornumber := NULL; errorstate := NULL;
        errorprocedure := 'sp_fuelsurcharge_batch_merge_proc';
        errorline := NULL; errormessage := NULL;
        RETURN;
    END IF;

    -- Calculate total batches
    v_total_batches := CEIL(v_total_staging::numeric / batch_size);

    -- Create numbered staging table for efficient batching
    DROP TABLE IF EXISTS stg_numbered;
    CREATE TEMP TABLE stg_numbered (
        rn bigint PRIMARY KEY,
        gpn_xpt_cny_cd char(4),
        gpn_ipt_cny_cd char(4),
        asy_svc_typ_cd char(3),
        mvm_drc_cd char(1),
        svc_typ_cd char(3),
        svc_fea_typ_cd char(3),
        pkg_cha_typ_cd char(3),
        pkg_acq_mth_typ_cd char(3),
        ccy_cd char(3),
        bil_ter_typ_cd char(3),
        ccl_mth_typ_cd char(2),
        asy_svc_ra_eff_dt date,
        asy_svc_ra_end_dt date,
        asy_svc_ra decimal(17,4),
        asy_svc_min_amt decimal(17,4),
        svc_ra_cht_sts_cd char(2),
        cus_csf_typ_cd char(2),
        load_ref_te character varying(100)
    );

    INSERT INTO stg_numbered
    SELECT ROW_NUMBER() OVER () AS rn,
           gpn_xpt_cny_cd, gpn_ipt_cny_cd, asy_svc_typ_cd, mvm_drc_cd,
           svc_typ_cd, svc_fea_typ_cd, pkg_cha_typ_cd, pkg_acq_mth_typ_cd,
           ccy_cd, bil_ter_typ_cd, ccl_mth_typ_cd, asy_svc_ra_eff_dt,
           asy_svc_ra_end_dt, asy_svc_ra, asy_svc_min_amt, svc_ra_cht_sts_cd,
           cus_csf_typ_cd, load_ref_te
    FROM tsubchg_stg;

    -- Create batch temp table
    DROP TABLE IF EXISTS batch_data;
    CREATE TEMP TABLE batch_data (LIKE tsubchg_stg INCLUDING ALL);

    -- Process batches
    WHILE v_offset < v_total_staging LOOP
        v_batch_count := v_batch_count + 1;
        v_batch_insert := 0;
        v_batch_update := 0;
        v_batch_matched_no_change := 0;

        TRUNCATE TABLE batch_data;

        INSERT INTO batch_data (
            gpn_xpt_cny_cd, gpn_ipt_cny_cd, asy_svc_typ_cd, mvm_drc_cd,
            svc_typ_cd, svc_fea_typ_cd, pkg_cha_typ_cd, pkg_acq_mth_typ_cd,
            ccy_cd, bil_ter_typ_cd, ccl_mth_typ_cd, asy_svc_ra_eff_dt,
            asy_svc_ra_end_dt, asy_svc_ra, asy_svc_min_amt, svc_ra_cht_sts_cd,
            cus_csf_typ_cd, load_ref_te
        )
        SELECT 
            gpn_xpt_cny_cd, gpn_ipt_cny_cd, asy_svc_typ_cd, mvm_drc_cd,
            svc_typ_cd, svc_fea_typ_cd, pkg_cha_typ_cd, pkg_acq_mth_typ_cd,
            ccy_cd, bil_ter_typ_cd, ccl_mth_typ_cd, asy_svc_ra_eff_dt,
            asy_svc_ra_end_dt, asy_svc_ra, asy_svc_min_amt, svc_ra_cht_sts_cd,
            cus_csf_typ_cd, load_ref_te
        FROM stg_numbered
        WHERE rn > v_offset AND rn <= v_offset + batch_size;

        WITH merge_result AS (
            MERGE INTO tsubchg AS tgt
            USING batch_data AS src
            ON tgt.gpn_xpt_cny_cd     = src.gpn_xpt_cny_cd
               AND tgt.gpn_ipt_cny_cd     = src.gpn_ipt_cny_cd
               AND tgt.asy_svc_typ_cd     = src.asy_svc_typ_cd
               AND tgt.mvm_drc_cd         = src.mvm_drc_cd
               AND tgt.svc_typ_cd         = src.svc_typ_cd
               AND tgt.svc_fea_typ_cd     = src.svc_fea_typ_cd
               AND tgt.pkg_cha_typ_cd     = src.pkg_cha_typ_cd
               AND tgt.pkg_acq_mth_typ_cd = src.pkg_acq_mth_typ_cd
               AND tgt.ccy_cd             = src.ccy_cd
               AND tgt.bil_ter_typ_cd     = src.bil_ter_typ_cd
               AND tgt.asy_svc_ra_eff_dt  = src.asy_svc_ra_eff_dt
               AND tgt.svc_ra_cht_sts_cd  = src.svc_ra_cht_sts_cd
               AND tgt.cus_csf_typ_cd     = src.cus_csf_typ_cd
            WHEN MATCHED AND (tgt.ccl_mth_typ_cd, tgt.asy_svc_ra_end_dt, tgt.asy_svc_ra, tgt.asy_svc_min_amt)
                IS DISTINCT FROM (src.ccl_mth_typ_cd, src.asy_svc_ra_end_dt, src.asy_svc_ra, src.asy_svc_min_amt) THEN
                UPDATE SET
                    ccl_mth_typ_cd     = src.ccl_mth_typ_cd,
                    asy_svc_ra_end_dt  = src.asy_svc_ra_end_dt,
                    asy_svc_ra         = src.asy_svc_ra,
                    asy_svc_min_amt    = src.asy_svc_min_amt,
                    load_ref_te        = src.load_ref_te
            WHEN MATCHED THEN
                DO NOTHING
            WHEN NOT MATCHED THEN
                INSERT (
                    gpn_xpt_cny_cd, gpn_ipt_cny_cd, asy_svc_typ_cd, mvm_drc_cd,
                    svc_typ_cd, svc_fea_typ_cd, pkg_cha_typ_cd, pkg_acq_mth_typ_cd,
                    ccy_cd, bil_ter_typ_cd, ccl_mth_typ_cd, asy_svc_ra_eff_dt,
                    asy_svc_ra_end_dt, asy_svc_ra, asy_svc_min_amt, svc_ra_cht_sts_cd,
                    cus_csf_typ_cd, load_ref_te
                )
                VALUES (
                    src.gpn_xpt_cny_cd, src.gpn_ipt_cny_cd, src.asy_svc_typ_cd, src.mvm_drc_cd,
                    src.svc_typ_cd, src.svc_fea_typ_cd, src.pkg_cha_typ_cd, src.pkg_acq_mth_typ_cd,
                    src.ccy_cd, src.bil_ter_typ_cd, src.ccl_mth_typ_cd, src.asy_svc_ra_eff_dt,
                    src.asy_svc_ra_end_dt, src.asy_svc_ra, src.asy_svc_min_amt, src.svc_ra_cht_sts_cd,
                    src.cus_csf_typ_cd, src.load_ref_te
                )
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
        DELETE FROM tsubchg t
        WHERE t.ctid IN (
            SELECT t2.ctid
            FROM tsubchg t2
            WHERE NOT EXISTS (
                SELECT 1 FROM tsubchg_stg s
                WHERE t2.gpn_xpt_cny_cd     = s.gpn_xpt_cny_cd
                  AND t2.gpn_ipt_cny_cd     = s.gpn_ipt_cny_cd
                  AND t2.asy_svc_typ_cd     = s.asy_svc_typ_cd
                  AND t2.mvm_drc_cd         = s.mvm_drc_cd
                  AND t2.svc_typ_cd         = s.svc_typ_cd
                  AND t2.svc_fea_typ_cd     = s.svc_fea_typ_cd
                  AND t2.pkg_cha_typ_cd     = s.pkg_cha_typ_cd
                  AND t2.pkg_acq_mth_typ_cd = s.pkg_acq_mth_typ_cd
                  AND t2.ccy_cd             = s.ccy_cd
                  AND t2.bil_ter_typ_cd     = s.bil_ter_typ_cd
                  AND t2.asy_svc_ra_eff_dt  = s.asy_svc_ra_eff_dt
                  AND t2.svc_ra_cht_sts_cd  = s.svc_ra_cht_sts_cd
                  AND t2.cus_csf_typ_cd     = s.cus_csf_typ_cd
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
    ErrorProcedure := 'sp_fuelsurcharge_batch_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    DROP TABLE IF EXISTS batch_data;
    DROP TABLE IF EXISTS stg_numbered;

    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_fuelsurcharge_batch_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
