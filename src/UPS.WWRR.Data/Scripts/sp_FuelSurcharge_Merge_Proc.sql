CREATE OR REPLACE PROCEDURE sp_fuelsurcharge_merge_proc(
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
        cus_csf_typ_cd char(2)
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               gpn_xpt_cny_cd,
               gpn_ipt_cny_cd,
               asy_svc_typ_cd,
               mvm_drc_cd,
               svc_typ_cd,
               svc_fea_typ_cd,
               pkg_cha_typ_cd,
               pkg_acq_mth_typ_cd,
               ccy_cd,
               bil_ter_typ_cd,
               ccl_mth_typ_cd,
               asy_svc_ra_eff_dt,
               asy_svc_ra_end_dt,
               asy_svc_ra,
               asy_svc_min_amt,
               svc_ra_cht_sts_cd,
               cus_csf_typ_cd,
               load_ref_te
          FROM tsubchg_stg
    ),
    cc_merge AS (
        MERGE INTO tsubchg AS tgt
        USING src_dedup AS src
        ON (
            tgt.gpn_xpt_cny_cd     = src.gpn_xpt_cny_cd AND
            tgt.gpn_ipt_cny_cd     = src.gpn_ipt_cny_cd AND
            tgt.asy_svc_typ_cd     = src.asy_svc_typ_cd AND
            tgt.mvm_drc_cd         = src.mvm_drc_cd AND
            tgt.svc_typ_cd         = src.svc_typ_cd AND
            tgt.svc_fea_typ_cd     = src.svc_fea_typ_cd AND
            tgt.pkg_cha_typ_cd     = src.pkg_cha_typ_cd AND
            tgt.pkg_acq_mth_typ_cd = src.pkg_acq_mth_typ_cd AND
            tgt.ccy_cd             = src.ccy_cd AND
            tgt.bil_ter_typ_cd     = src.bil_ter_typ_cd AND
            tgt.asy_svc_ra_eff_dt  = src.asy_svc_ra_eff_dt AND
            tgt.svc_ra_cht_sts_cd  = src.svc_ra_cht_sts_cd AND
            tgt.cus_csf_typ_cd     = src.cus_csf_typ_cd
        )
        WHEN MATCHED AND (
            tgt.ccl_mth_typ_cd    IS DISTINCT FROM src.ccl_mth_typ_cd OR 
            tgt.asy_svc_ra_end_dt IS DISTINCT FROM src.asy_svc_ra_end_dt OR 
            tgt.asy_svc_ra        IS DISTINCT FROM src.asy_svc_ra OR 
            tgt.asy_svc_min_amt   IS DISTINCT FROM src.asy_svc_min_amt
        ) THEN UPDATE SET
            ccl_mth_typ_cd     = src.ccl_mth_typ_cd, 
            asy_svc_ra_end_dt  = src.asy_svc_ra_end_dt, 
            asy_svc_ra         = src.asy_svc_ra, 
            asy_svc_min_amt    = src.asy_svc_min_amt, 
            load_ref_te        = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                gpn_xpt_cny_cd,
                gpn_ipt_cny_cd,
                asy_svc_typ_cd,
                mvm_drc_cd,
                svc_typ_cd,
                svc_fea_typ_cd,
                pkg_cha_typ_cd,
                pkg_acq_mth_typ_cd,
                ccy_cd,
                bil_ter_typ_cd,
                ccl_mth_typ_cd,
                asy_svc_ra_eff_dt,
                asy_svc_ra_end_dt,
                asy_svc_ra,
                asy_svc_min_amt,
                svc_ra_cht_sts_cd,
                cus_csf_typ_cd,
                load_ref_te
            ) VALUES (
                src.gpn_xpt_cny_cd,
                src.gpn_ipt_cny_cd,
                src.asy_svc_typ_cd,
                src.mvm_drc_cd,
                src.svc_typ_cd,
                src.svc_fea_typ_cd,
                src.pkg_cha_typ_cd,
                src.pkg_acq_mth_typ_cd,
                src.ccy_cd,
                src.bil_ter_typ_cd,
                src.ccl_mth_typ_cd,
                src.asy_svc_ra_eff_dt,
                src.asy_svc_ra_end_dt,
                src.asy_svc_ra,
                src.asy_svc_min_amt,
                src.svc_ra_cht_sts_cd,
                src.cus_csf_typ_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.gpn_xpt_cny_cd,     src.gpn_xpt_cny_cd)     AS gpn_xpt_cny_cd,
            COALESCE(tgt.gpn_ipt_cny_cd,     src.gpn_ipt_cny_cd)     AS gpn_ipt_cny_cd,
            COALESCE(tgt.asy_svc_typ_cd,     src.asy_svc_typ_cd)     AS asy_svc_typ_cd,
            COALESCE(tgt.mvm_drc_cd,         src.mvm_drc_cd)         AS mvm_drc_cd,
            COALESCE(tgt.svc_typ_cd,         src.svc_typ_cd)         AS svc_typ_cd,
            COALESCE(tgt.svc_fea_typ_cd,     src.svc_fea_typ_cd)     AS svc_fea_typ_cd,
            COALESCE(tgt.pkg_cha_typ_cd,     src.pkg_cha_typ_cd)     AS pkg_cha_typ_cd,
            COALESCE(tgt.pkg_acq_mth_typ_cd, src.pkg_acq_mth_typ_cd) AS pkg_acq_mth_typ_cd,
            COALESCE(tgt.ccy_cd,             src.ccy_cd)             AS ccy_cd,
            COALESCE(tgt.bil_ter_typ_cd,     src.bil_ter_typ_cd)     AS bil_ter_typ_cd,
            COALESCE(tgt.ccl_mth_typ_cd,     src.ccl_mth_typ_cd)     AS ccl_mth_typ_cd,
            COALESCE(tgt.asy_svc_ra_eff_dt,  src.asy_svc_ra_eff_dt)  AS asy_svc_ra_eff_dt,
            COALESCE(tgt.asy_svc_ra_end_dt,  src.asy_svc_ra_end_dt)  AS asy_svc_ra_end_dt,
            COALESCE(tgt.asy_svc_ra,         src.asy_svc_ra)         AS asy_svc_ra,
            COALESCE(tgt.asy_svc_min_amt,    src.asy_svc_min_amt)    AS asy_svc_min_amt,
            COALESCE(tgt.svc_ra_cht_sts_cd,  src.svc_ra_cht_sts_cd)  AS svc_ra_cht_sts_cd,
            COALESCE(tgt.cus_csf_typ_cd,     src.cus_csf_typ_cd)     AS cus_csf_typ_cd
)
    INSERT INTO merge_actions (
        table_name, 
        action,
        gpn_xpt_cny_cd,
        gpn_ipt_cny_cd,
        asy_svc_typ_cd,
        mvm_drc_cd,
        svc_typ_cd,
        svc_fea_typ_cd,
        pkg_cha_typ_cd,
        pkg_acq_mth_typ_cd,
        ccy_cd,
        bil_ter_typ_cd,
        ccl_mth_typ_cd,
        asy_svc_ra_eff_dt,
        asy_svc_ra_end_dt,
        asy_svc_ra,
        asy_svc_min_amt,
        svc_ra_cht_sts_cd,
        cus_csf_typ_cd
    )
    SELECT 
        'tsubchg', 
        merge_action,
        gpn_xpt_cny_cd,
        gpn_ipt_cny_cd,
        asy_svc_typ_cd,
        mvm_drc_cd,
        svc_typ_cd,
        svc_fea_typ_cd,
        pkg_cha_typ_cd,
        pkg_acq_mth_typ_cd,
        ccy_cd,
        bil_ter_typ_cd,
        ccl_mth_typ_cd,
        asy_svc_ra_eff_dt,
        asy_svc_ra_end_dt,
        asy_svc_ra,
        asy_svc_min_amt,
        svc_ra_cht_sts_cd,
        cus_csf_typ_cd
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_fuelsurcharge_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_fuelsurcharge_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
