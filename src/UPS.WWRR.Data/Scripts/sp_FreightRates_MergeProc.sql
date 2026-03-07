CREATE OR REPLACE PROCEDURE sp_freightrates_merge_proc(
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
BEGIN
    CREATE TEMP TABLE merge_actions (
        table_name          text,
        action              text,
        svc_ra_cht_nr       char(6),
        svc_ra_cht_eff_dt   date,
        svc_ra_cht_sts_cd   char(2),
        cmy_cls_cd          char(4),
        ccl_mth_typ_cd      char(2),
        del_zn_nr           char(6),
        wgt_ms_unt_typ_cd   char(2),
        wgt_cgy_min_wgt_qy  float8
    );

    WITH src_dedup AS (
        SELECT DISTINCT
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
          FROM trastd_stg
    ),
    cc_merge AS (
        MERGE INTO trastd AS tgt
        USING src_dedup AS src
        ON (
            tgt.svc_ra_cht_nr       = src.svc_ra_cht_nr AND
            tgt.svc_ra_cht_eff_dt   = src.svc_ra_cht_eff_dt AND
            tgt.svc_ra_cht_sts_cd   = src.svc_ra_cht_sts_cd AND
            tgt.cmy_cls_cd          = src.cmy_cls_cd AND
            tgt.ccl_mth_typ_cd      = src.ccl_mth_typ_cd AND
            tgt.del_zn_nr           = src.del_zn_nr AND
            tgt.wgt_ms_unt_typ_cd   = src.wgt_ms_unt_typ_cd AND
            tgt.wgt_cgy_min_wgt_qy  = src.wgt_cgy_min_wgt_qy
        )
        WHEN MATCHED AND (
            tgt.wgt_cgy_max_wgt_qy  IS DISTINCT FROM src.wgt_cgy_max_wgt_qy OR
            tgt.ac_spl_bil_ter_pr   IS DISTINCT FROM src.ac_spl_bil_ter_pr OR
            tgt.cns_spl_bil_ter_pr  IS DISTINCT FROM src.cns_spl_bil_ter_pr OR
            tgt.svc_ra_cht_end_dt   IS DISTINCT FROM src.svc_ra_cht_end_dt
        ) THEN UPDATE SET
            wgt_cgy_max_wgt_qy  = src.wgt_cgy_max_wgt_qy,
            ac_spl_bil_ter_pr   = src.ac_spl_bil_ter_pr,
            cns_spl_bil_ter_pr  = src.cns_spl_bil_ter_pr,
            svc_ra_cht_end_dt   = src.svc_ra_cht_end_dt,
            load_ref_te         = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
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
            ) VALUES (
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
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.svc_ra_cht_nr,      src.svc_ra_cht_nr)      AS svc_ra_cht_nr,
            COALESCE(tgt.svc_ra_cht_eff_dt,  src.svc_ra_cht_eff_dt)  AS svc_ra_cht_eff_dt,
            COALESCE(tgt.svc_ra_cht_sts_cd,  src.svc_ra_cht_sts_cd)  AS svc_ra_cht_sts_cd,
            COALESCE(tgt.cmy_cls_cd,         src.cmy_cls_cd)         AS cmy_cls_cd,
            COALESCE(tgt.ccl_mth_typ_cd,     src.ccl_mth_typ_cd)     AS ccl_mth_typ_cd,
            COALESCE(tgt.del_zn_nr,          src.del_zn_nr)          AS del_zn_nr,
            COALESCE(tgt.wgt_ms_unt_typ_cd,  src.wgt_ms_unt_typ_cd)  AS wgt_ms_unt_typ_cd,
            COALESCE(tgt.wgt_cgy_min_wgt_qy, src.wgt_cgy_min_wgt_qy) AS wgt_cgy_min_wgt_qy
    )
    INSERT INTO merge_actions (
        table_name, action,
        svc_ra_cht_nr, svc_ra_cht_eff_dt, svc_ra_cht_sts_cd, cmy_cls_cd,
        ccl_mth_typ_cd, del_zn_nr, wgt_ms_unt_typ_cd, wgt_cgy_min_wgt_qy
    )
    SELECT 'trastd', merge_action,
           svc_ra_cht_nr, svc_ra_cht_eff_dt, svc_ra_cht_sts_cd, cmy_cls_cd,
           ccl_mth_typ_cd, del_zn_nr, wgt_ms_unt_typ_cd, wgt_cgy_min_wgt_qy
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_freightrates_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_freightrates_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
