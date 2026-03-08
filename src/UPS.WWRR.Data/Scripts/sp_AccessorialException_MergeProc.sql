CREATE OR REPLACE PROCEDURE sp_accessorialexception_merge_proc(
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
        brl_typ_cd text,
        gpn_xpt_cny_cd text,
        gpn_ipt_cny_cd text,
        pkg_cha_typ_cd text,
        bil_ter_typ_cd text,
        svc_fea_typ_cd text,
        svc_typ_cd text,
        mvm_drc_cd text,
        cus_csf_typ_cd text,
        asy_svc_typ_cd text,
        apv_sts_cd text,
        rec_eff_stt_dt date
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               brl_typ_cd,
               gpn_xpt_cny_cd,
               gpn_ipt_cny_cd,
               pkg_cha_typ_cd,
               bil_ter_typ_cd,
               svc_fea_typ_cd,
               svc_typ_cd,
               mvm_drc_cd,
               cus_csf_typ_cd,
               asy_svc_typ_cd,
               apv_sts_cd,
               rec_eff_stt_dt,
               rec_eff_end_dt,
               load_ref_te
          FROM tasybrl_stg
    ),
    cc_merge AS (
        MERGE INTO tasybrl AS tgt
        USING src_dedup AS src
        ON (
            tgt.brl_typ_cd        = src.brl_typ_cd AND
            tgt.gpn_xpt_cny_cd    = src.gpn_xpt_cny_cd AND
            tgt.gpn_ipt_cny_cd    = src.gpn_ipt_cny_cd AND
            tgt.pkg_cha_typ_cd    = src.pkg_cha_typ_cd AND
            tgt.bil_ter_typ_cd    = src.bil_ter_typ_cd AND
            tgt.svc_fea_typ_cd    = src.svc_fea_typ_cd AND
            tgt.svc_typ_cd        = src.svc_typ_cd AND
            tgt.mvm_drc_cd        = src.mvm_drc_cd AND
            tgt.cus_csf_typ_cd    = src.cus_csf_typ_cd AND
            tgt.asy_svc_typ_cd    = src.asy_svc_typ_cd AND
            tgt.apv_sts_cd        = src.apv_sts_cd AND
            tgt.rec_eff_stt_dt    = src.rec_eff_stt_dt
        )
        WHEN MATCHED AND (
            tgt.rec_eff_end_dt IS DISTINCT FROM src.rec_eff_end_dt
        ) THEN UPDATE SET
            rec_eff_end_dt = src.rec_eff_end_dt,
            load_ref_te    = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                brl_typ_cd,
                gpn_xpt_cny_cd,
                gpn_ipt_cny_cd,
                pkg_cha_typ_cd,
                bil_ter_typ_cd,
                svc_fea_typ_cd,
                svc_typ_cd,
                mvm_drc_cd,
                cus_csf_typ_cd,
                asy_svc_typ_cd,
                apv_sts_cd,
                rec_eff_stt_dt,
                rec_eff_end_dt,
                load_ref_te
            ) VALUES (
                src.brl_typ_cd,
                src.gpn_xpt_cny_cd,
                src.gpn_ipt_cny_cd,
                src.pkg_cha_typ_cd,
                src.bil_ter_typ_cd,
                src.svc_fea_typ_cd,
                src.svc_typ_cd,
                src.mvm_drc_cd,
                src.cus_csf_typ_cd,
                src.asy_svc_typ_cd,
                src.apv_sts_cd,
                src.rec_eff_stt_dt,
                src.rec_eff_end_dt,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.brl_typ_cd,     src.brl_typ_cd)     AS brl_typ_cd,
            COALESCE(tgt.gpn_xpt_cny_cd, src.gpn_xpt_cny_cd) AS gpn_xpt_cny_cd,
            COALESCE(tgt.gpn_ipt_cny_cd, src.gpn_ipt_cny_cd) AS gpn_ipt_cny_cd,
            COALESCE(tgt.pkg_cha_typ_cd, src.pkg_cha_typ_cd) AS pkg_cha_typ_cd,
            COALESCE(tgt.bil_ter_typ_cd, src.bil_ter_typ_cd) AS bil_ter_typ_cd,
            COALESCE(tgt.svc_fea_typ_cd, src.svc_fea_typ_cd) AS svc_fea_typ_cd,
            COALESCE(tgt.svc_typ_cd,     src.svc_typ_cd)     AS svc_typ_cd,
            COALESCE(tgt.mvm_drc_cd,     src.mvm_drc_cd)     AS mvm_drc_cd,
            COALESCE(tgt.cus_csf_typ_cd, src.cus_csf_typ_cd) AS cus_csf_typ_cd,
            COALESCE(tgt.asy_svc_typ_cd, src.asy_svc_typ_cd) AS asy_svc_typ_cd,
            COALESCE(tgt.apv_sts_cd,     src.apv_sts_cd)     AS apv_sts_cd,
            COALESCE(tgt.rec_eff_stt_dt, src.rec_eff_stt_dt) AS rec_eff_stt_dt
    )
    INSERT INTO merge_actions (
        table_name, action,
        brl_typ_cd, gpn_xpt_cny_cd, gpn_ipt_cny_cd, pkg_cha_typ_cd, bil_ter_typ_cd, svc_fea_typ_cd, svc_typ_cd, mvm_drc_cd, cus_csf_typ_cd, asy_svc_typ_cd, apv_sts_cd, rec_eff_stt_dt
    )
    SELECT
        'tasybrl', merge_action,
        brl_typ_cd, gpn_xpt_cny_cd, gpn_ipt_cny_cd, pkg_cha_typ_cd, bil_ter_typ_cd, svc_fea_typ_cd, svc_typ_cd, mvm_drc_cd, cus_csf_typ_cd, asy_svc_typ_cd, apv_sts_cd, rec_eff_stt_dt
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_accessorialexception_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_accessorialexception_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;