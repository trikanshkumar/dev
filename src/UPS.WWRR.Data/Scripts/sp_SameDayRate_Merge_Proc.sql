CREATE OR REPLACE PROCEDURE sp_samedayrate_merge_proc(
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
        table_name text,
        action     text,
        org_gpn_cd char(4),
        dtn_gpn_cd char(4),
        svc_typ_cd char(3),
        del_zn_nr char(6),
        ccy_cd char(3),
        pkg_cha_typ_cd char(3),
        cus_csf_typ_cd char(2),
        svc_fea_typ_cd char(3),
        cny_ra_sei_rl_cd char(1),
        wgt_ms_unt_typ_cd char(2),
        wgt_cgy_max_wgt_qy decimal(9,2),
        wgt_cgy_min_wgt_qy decimal(9,2),
        svc_ra_cht_sts_cd char(2),
        svc_ra_cht_eff_dt date,
        svc_ra_cht_end_dt date,
        ccl_mth_typ_cd char(2),
        dtr_cri_ra_a decimal(17,4),
        svc_ra_cht_nr char(7)
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               org_gpn_cd,
               dtn_gpn_cd,
               svc_typ_cd,
               del_zn_nr,
               ccy_cd,
               pkg_cha_typ_cd,
               cus_csf_typ_cd,
               svc_fea_typ_cd,
               cny_ra_sei_rl_cd,
               wgt_ms_unt_typ_cd,
               wgt_cgy_max_wgt_qy,
               wgt_cgy_min_wgt_qy,
               svc_ra_cht_sts_cd,
               svc_ra_cht_eff_dt,
               svc_ra_cht_end_dt,
               ccl_mth_typ_cd,
               dtr_cri_ra_a,
               svc_ra_cht_nr,
               load_ref_te
          FROM tsdrwsf_stg
    ),
    cc_merge AS (
        MERGE INTO tsdrwsf AS tgt
        USING src_dedup AS src
        ON (
            tgt.org_gpn_cd         = src.org_gpn_cd AND
            tgt.dtn_gpn_cd         = src.dtn_gpn_cd AND
            tgt.svc_typ_cd         = src.svc_typ_cd AND
            tgt.del_zn_nr          = src.del_zn_nr AND
            tgt.ccy_cd             = src.ccy_cd AND
            tgt.pkg_cha_typ_cd     = src.pkg_cha_typ_cd AND 
            tgt.cus_csf_typ_cd     = src.cus_csf_typ_cd AND 
            tgt.svc_fea_typ_cd     = src.svc_fea_typ_cd AND 
            tgt.cny_ra_sei_rl_cd   = src.cny_ra_sei_rl_cd AND
            tgt.wgt_ms_unt_typ_cd  = src.wgt_ms_unt_typ_cd AND
            tgt.wgt_cgy_min_wgt_qy = src.wgt_cgy_min_wgt_qy AND
            tgt.svc_ra_cht_sts_cd  = src.svc_ra_cht_sts_cd AND
            tgt.svc_ra_cht_eff_dt  = src.svc_ra_cht_eff_dt
        )
        WHEN MATCHED AND (
            tgt.wgt_cgy_max_wgt_qy  IS DISTINCT FROM src.wgt_cgy_max_wgt_qy OR
            tgt.svc_ra_cht_end_dt   IS DISTINCT FROM src.svc_ra_cht_end_dt  OR
            tgt.ccl_mth_typ_cd      IS DISTINCT FROM src.ccl_mth_typ_cd     OR
            tgt.dtr_cri_ra_a        IS DISTINCT FROM src.dtr_cri_ra_a       OR
            tgt.svc_ra_cht_nr       IS DISTINCT FROM src.svc_ra_cht_nr
        ) THEN UPDATE SET
            wgt_cgy_max_wgt_qy = src.wgt_cgy_max_wgt_qy,
            svc_ra_cht_end_dt  = src.svc_ra_cht_end_dt,
            ccl_mth_typ_cd     = src.ccl_mth_typ_cd,
            dtr_cri_ra_a       = src.dtr_cri_ra_a,
            svc_ra_cht_nr      = src.svc_ra_cht_nr,
            load_ref_te        = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                org_gpn_cd,
                dtn_gpn_cd,
                svc_typ_cd,
                del_zn_nr,
                ccy_cd,
                pkg_cha_typ_cd, 
                cus_csf_typ_cd, 
                svc_fea_typ_cd, 
                cny_ra_sei_rl_cd,
                wgt_ms_unt_typ_cd,
                wgt_cgy_max_wgt_qy,
                wgt_cgy_min_wgt_qy,
                svc_ra_cht_sts_cd,
                svc_ra_cht_eff_dt,
                svc_ra_cht_end_dt,
                ccl_mth_typ_cd,
                dtr_cri_ra_a,
                svc_ra_cht_nr,
                load_ref_te
            ) VALUES (
                src.org_gpn_cd,
                src.dtn_gpn_cd,
                src.svc_typ_cd,
                src.del_zn_nr,
                src.ccy_cd,
                src.pkg_cha_typ_cd, 
                src.cus_csf_typ_cd, 
                src.svc_fea_typ_cd, 
                src.cny_ra_sei_rl_cd,
                src.wgt_ms_unt_typ_cd,
                src.wgt_cgy_max_wgt_qy,
                src.wgt_cgy_min_wgt_qy,
                src.svc_ra_cht_sts_cd,
                src.svc_ra_cht_eff_dt,
                src.svc_ra_cht_end_dt,
                src.ccl_mth_typ_cd,
                src.dtr_cri_ra_a,
                src.svc_ra_cht_nr,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.org_gpn_cd,         src.org_gpn_cd)         AS org_gpn_cd,
            COALESCE(tgt.dtn_gpn_cd,         src.dtn_gpn_cd)         AS dtn_gpn_cd,
            COALESCE(tgt.svc_typ_cd,         src.svc_typ_cd)         AS svc_typ_cd,
            COALESCE(tgt.del_zn_nr,          src.del_zn_nr)          AS del_zn_nr,
            COALESCE(tgt.ccy_cd,             src.ccy_cd)             AS ccy_cd,
            COALESCE(tgt.pkg_cha_typ_cd,     src.pkg_cha_typ_cd)     AS pkg_cha_typ_cd,
            COALESCE(tgt.cus_csf_typ_cd,     src.cus_csf_typ_cd)     AS cus_csf_typ_cd,
            COALESCE(tgt.svc_fea_typ_cd,     src.svc_fea_typ_cd)     AS svc_fea_typ_cd,
            COALESCE(tgt.cny_ra_sei_rl_cd,   src.cny_ra_sei_rl_cd)   AS cny_ra_sei_rl_cd,
            COALESCE(tgt.wgt_ms_unt_typ_cd,  src.wgt_ms_unt_typ_cd)  AS wgt_ms_unt_typ_cd,
            COALESCE(tgt.wgt_cgy_max_wgt_qy, src.wgt_cgy_max_wgt_qy) AS wgt_cgy_max_wgt_qy,
            COALESCE(tgt.wgt_cgy_min_wgt_qy, src.wgt_cgy_min_wgt_qy) AS wgt_cgy_min_wgt_qy,
            COALESCE(tgt.svc_ra_cht_sts_cd,  src.svc_ra_cht_sts_cd)  AS svc_ra_cht_sts_cd,
            COALESCE(tgt.svc_ra_cht_eff_dt,  src.svc_ra_cht_eff_dt)  AS svc_ra_cht_eff_dt,
            COALESCE(tgt.svc_ra_cht_end_dt,  src.svc_ra_cht_end_dt)  AS svc_ra_cht_end_dt,
            COALESCE(tgt.ccl_mth_typ_cd,     src.ccl_mth_typ_cd)     AS ccl_mth_typ_cd,
            COALESCE(tgt.dtr_cri_ra_a,       src.dtr_cri_ra_a)       AS dtr_cri_ra_a,
            COALESCE(tgt.svc_ra_cht_nr,      src.svc_ra_cht_nr)      AS svc_ra_cht_nr
)
    INSERT INTO merge_actions (
        table_name, 
        action,
        org_gpn_cd,
        dtn_gpn_cd,
        svc_typ_cd,
        del_zn_nr,
        ccy_cd,
        pkg_cha_typ_cd, 
        cus_csf_typ_cd, 
        svc_fea_typ_cd, 
        cny_ra_sei_rl_cd,
        wgt_ms_unt_typ_cd,
        wgt_cgy_max_wgt_qy,
        wgt_cgy_min_wgt_qy,
        svc_ra_cht_sts_cd,
        svc_ra_cht_eff_dt,
        svc_ra_cht_end_dt,
        ccl_mth_typ_cd,
        dtr_cri_ra_a,
        svc_ra_cht_nr
    )
    SELECT 
        'tsdrwsf', 
        merge_action,
        org_gpn_cd,
        dtn_gpn_cd,
        svc_typ_cd,
        del_zn_nr,
        ccy_cd,
        pkg_cha_typ_cd, 
        cus_csf_typ_cd, 
        svc_fea_typ_cd, 
        cny_ra_sei_rl_cd,
        wgt_ms_unt_typ_cd,
        wgt_cgy_max_wgt_qy,
        wgt_cgy_min_wgt_qy,
        svc_ra_cht_sts_cd,
        svc_ra_cht_eff_dt,
        svc_ra_cht_end_dt,
        ccl_mth_typ_cd,
        dtr_cri_ra_a,
        svc_ra_cht_nr
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_samedayrate_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_samedayrate_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
