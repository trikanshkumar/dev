CREATE OR REPLACE PROCEDURE sp_freightrates_batch_merge_proc(
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
    SELECT COUNT(*) INTO v_total_staging FROM trastd_stg;

    -- Handle empty staging
    IF v_total_staging = 0 THEN
        errornumber := NULL; errorstate := NULL;
        errorprocedure := 'sp_freightrates_batch_merge_proc';
        errorline := NULL; errormessage := NULL;
        RETURN;
    END IF;

    -- Calculate total batches
    v_total_batches := CEIL(v_total_staging::numeric / batch_size);

    -- Create numbered staging table for efficient batching
    DROP TABLE IF EXISTS stg_numbered;
    CREATE TEMP TABLE stg_numbered (
        rn bigint PRIMARY KEY,
        svc_ra_cht_nr       char(6),
        svc_ra_cht_eff_dt   date,
        svc_ra_cht_sts_cd   char(2),
        cmy_cls_cd          char(4),
        ccl_mth_typ_cd      char(2),
        del_zn_nr           char(6),
        wgt_ms_unt_typ_cd   char(2),
        wgt_cgy_min_wgt_qy  float8,
        wgt_cgy_max_wgt_qy  float8,
        ac_spl_bil_ter_pr   decimal(17,4),
        cns_spl_bil_ter_pr  decimal(17,4),
        svc_ra_cht_end_dt   date,
        load_ref_te         char(255)
    );

    INSERT INTO stg_numbered
    SELECT ROW_NUMBER() OVER () AS rn,
        svc_ra_cht_nr,
        svc_ra_cht_eff_dt,
        svc_ra_cht_sts_cd,
        cmy_cls_cd,
        ccl_mth_typ_cd,
        del_zn_nr,
        wgt_ms_unt_typ_cd,
        wgt_cgy_min_wgt_qy,
        wgt_cgy_max_wgt_qy,
        ac_spl_bil_ter_pr,
        cns_spl_bil_ter_pr,
        svc_ra_cht_end_dt,
        load_ref_te
    FROM trastd_stg;

    -- Create batch temp table
    DROP TABLE IF EXISTS batch_data;
    CREATE TEMP TABLE batch_data (LIKE trastd_stg INCLUDING ALL);

    -- Process batches
    WHILE v_offset < v_total_staging LOOP
        v_batch_count := v_batch_count + 1;
        v_batch_insert := 0;
        v_batch_update := 0;
        v_batch_matched_no_change := 0;

        TRUNCATE TABLE batch_data;

        INSERT INTO batch_data (
            svc_ra_cht_nr,
            svc_ra_cht_eff_dt,
            svc_ra_cht_sts_cd,
            cmy_cls_cd,
            ccl_mth_typ_cd,
            del_zn_nr,
            wgt_ms_unt_typ_cd,
            wgt_cgy_min_wgt_qy,
            wgt_cgy_max_wgt_qy,
            ac_spl_bil_ter_pr,
            cns_spl_bil_ter_pr,
            svc_ra_cht_end_dt,
            load_ref_te
        )
        SELECT 
            svc_ra_cht_nr,
            svc_ra_cht_eff_dt,
            svc_ra_cht_sts_cd,
            cmy_cls_cd,
            ccl_mth_typ_cd,
            del_zn_nr,
            wgt_ms_unt_typ_cd,
            wgt_cgy_min_wgt_qy,
            wgt_cgy_max_wgt_qy,
            ac_spl_bil_ter_pr,
            cns_spl_bil_ter_pr,
            svc_ra_cht_end_dt,
            load_ref_te
        FROM stg_numbered
        WHERE rn > v_offset AND rn <= v_offset + batch_size;

        WITH merge_result AS (
            MERGE INTO trastd AS tgt
            USING batch_data AS src
            ON tgt.svc_ra_cht_nr = src.svc_ra_cht_nr
               AND tgt.svc_ra_cht_eff_dt = src.svc_ra_cht_eff_dt
               AND tgt.svc_ra_cht_sts_cd = src.svc_ra_cht_sts_cd
               AND tgt.cmy_cls_cd = src.cmy_cls_cd
               AND tgt.ccl_mth_typ_cd = src.ccl_mth_typ_cd
               AND tgt.del_zn_nr = src.del_zn_nr
               AND tgt.wgt_ms_unt_typ_cd = src.wgt_ms_unt_typ_cd
               AND tgt.wgt_cgy_min_wgt_qy = src.wgt_cgy_min_wgt_qy
            WHEN MATCHED AND (
                    tgt.wgt_cgy_max_wgt_qy,
                    tgt.ac_spl_bil_ter_pr,
                    tgt.cns_spl_bil_ter_pr,
                    tgt.svc_ra_cht_end_dt
                )
                IS DISTINCT FROM (
                    src.wgt_cgy_max_wgt_qy,
                    src.ac_spl_bil_ter_pr,
                    src.cns_spl_bil_ter_pr,
                    src.svc_ra_cht_end_dt
                ) 
                THEN
                UPDATE SET
                    wgt_cgy_max_wgt_qy = src.wgt_cgy_max_wgt_qy,
                    ac_spl_bil_ter_pr = src.ac_spl_bil_ter_pr,
                    cns_spl_bil_ter_pr = src.cns_spl_bil_ter_pr,
                    svc_ra_cht_end_dt = src.svc_ra_cht_end_dt,
                    load_ref_te = src.load_ref_te
            WHEN MATCHED THEN
                DO NOTHING
            WHEN NOT MATCHED THEN
                INSERT (
                    svc_ra_cht_nr,
                    svc_ra_cht_eff_dt,
                    svc_ra_cht_sts_cd,
                    cmy_cls_cd,
                    ccl_mth_typ_cd,
                    del_zn_nr,
                    wgt_ms_unt_typ_cd,
                    wgt_cgy_min_wgt_qy,
                    wgt_cgy_max_wgt_qy,
                    ac_spl_bil_ter_pr,
                    cns_spl_bil_ter_pr,
                    svc_ra_cht_end_dt,
                    load_ref_te
                    )
                VALUES (
                    src.svc_ra_cht_nr,
                    src.svc_ra_cht_eff_dt,
                    src.svc_ra_cht_sts_cd,
                    src.cmy_cls_cd,
                    src.ccl_mth_typ_cd,
                    src.del_zn_nr,
                    src.wgt_ms_unt_typ_cd,
                    src.wgt_cgy_min_wgt_qy,
                    src.wgt_cgy_max_wgt_qy,
                    src.ac_spl_bil_ter_pr,
                    src.cns_spl_bil_ter_pr,
                    src.svc_ra_cht_end_dt,
                    src.load_ref_te
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
        DELETE FROM trastd t
        WHERE t.ctid IN (
            SELECT t2.ctid
            FROM trastd t2
            WHERE NOT EXISTS (
                SELECT 1 FROM trastd_stg s
                WHERE 
                t2.svc_ra_cht_nr = s.svc_ra_cht_nr
                AND t2.svc_ra_cht_eff_dt = s.svc_ra_cht_eff_dt
                AND t2.svc_ra_cht_sts_cd = s.svc_ra_cht_sts_cd
                AND t2.cmy_cls_cd = s.cmy_cls_cd
                AND t2.ccl_mth_typ_cd = s.ccl_mth_typ_cd
                AND t2.del_zn_nr = s.del_zn_nr
                AND t2.wgt_ms_unt_typ_cd = s.wgt_ms_unt_typ_cd
                AND t2.wgt_cgy_min_wgt_qy = s.wgt_cgy_min_wgt_qy
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
    ErrorProcedure := 'sp_freightrates_batch_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    DROP TABLE IF EXISTS batch_data;
    DROP TABLE IF EXISTS stg_numbered;

    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_freightrates_batch_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
