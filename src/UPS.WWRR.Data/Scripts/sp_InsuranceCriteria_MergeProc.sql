CREATE OR REPLACE PROCEDURE sp_insurancecriteria_merge_proc(
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
        cus_cls_typ_cd text,
        cny_cd text,
        asy_svc_typ_cd text,
        svc_fea_typ_cd text,
        wgt_ms_unt_typ_cd text,
        ccy_cd text,
        ins_cri_eff_stt_dt date,
        svc_typ_cd text,
        apv_sts_cd text
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               cus_cls_typ_cd,
               cny_cd,
               asy_svc_typ_cd,
               svc_fea_typ_cd,
               wgt_ms_unt_typ_cd,
               ccy_cd,
               ins_bss_a,
               min_ins_avail,
               ins_min_dcl_vlu,
               ins_min_wgt_qy,
               ins_max_wgt_qy,
               ins_cri_eff_stt_dt,
               ins_cri_eff_end_dt,
               svc_typ_cd,
               isn_max_dcl_vlu_a,
               apv_sts_cd,
               load_ref_te
          FROM tinscri_stg
    ),
    cc_merge AS (
        MERGE INTO tinscri AS tgt
        USING src_dedup AS src
        ON (
            tgt.cus_cls_typ_cd    = src.cus_cls_typ_cd AND
            tgt.cny_cd            = src.cny_cd AND
            tgt.asy_svc_typ_cd    = src.asy_svc_typ_cd AND
            tgt.svc_fea_typ_cd    = src.svc_fea_typ_cd AND
            tgt.wgt_ms_unt_typ_cd = src.wgt_ms_unt_typ_cd AND
            tgt.ccy_cd            = src.ccy_cd AND
            tgt.ins_cri_eff_stt_dt= src.ins_cri_eff_stt_dt AND
            tgt.svc_typ_cd        = src.svc_typ_cd AND
            tgt.apv_sts_cd        = src.apv_sts_cd
        )
        WHEN MATCHED AND (
            tgt.ins_bss_a         IS DISTINCT FROM src.ins_bss_a OR
            tgt.min_ins_avail     IS DISTINCT FROM src.min_ins_avail OR
            tgt.ins_min_dcl_vlu   IS DISTINCT FROM src.ins_min_dcl_vlu OR
            tgt.ins_min_wgt_qy    IS DISTINCT FROM src.ins_min_wgt_qy OR
            tgt.ins_max_wgt_qy    IS DISTINCT FROM src.ins_max_wgt_qy OR
            tgt.ins_cri_eff_end_dt IS DISTINCT FROM src.ins_cri_eff_end_dt OR
            tgt.isn_max_dcl_vlu_a IS DISTINCT FROM src.isn_max_dcl_vlu_a OR
            tgt.load_ref_te       IS DISTINCT FROM src.load_ref_te
        )
            THEN UPDATE SET
                ins_bss_a          = src.ins_bss_a,
                min_ins_avail      = src.min_ins_avail,
                ins_min_dcl_vlu    = src.ins_min_dcl_vlu,
                ins_min_wgt_qy     = src.ins_min_wgt_qy,
                ins_max_wgt_qy     = src.ins_max_wgt_qy,
                ins_cri_eff_end_dt = src.ins_cri_eff_end_dt,
                isn_max_dcl_vlu_a  = src.isn_max_dcl_vlu_a,
                load_ref_te        = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                cus_cls_typ_cd,
                cny_cd,
                asy_svc_typ_cd,
                svc_fea_typ_cd,
                wgt_ms_unt_typ_cd,
                ccy_cd,
                ins_bss_a,
                min_ins_avail,
                ins_min_dcl_vlu,
                ins_min_wgt_qy,
                ins_max_wgt_qy,
                ins_cri_eff_stt_dt,
                ins_cri_eff_end_dt,
                svc_typ_cd,
                isn_max_dcl_vlu_a,
                apv_sts_cd,
                load_ref_te
            ) VALUES (
                src.cus_cls_typ_cd,
                src.cny_cd,
                src.asy_svc_typ_cd,
                src.svc_fea_typ_cd,
                src.wgt_ms_unt_typ_cd,
                src.ccy_cd,
                src.ins_bss_a,
                src.min_ins_avail,
                src.ins_min_dcl_vlu,
                src.ins_min_wgt_qy,
                src.ins_max_wgt_qy,
                src.ins_cri_eff_stt_dt,
                src.ins_cri_eff_end_dt,
                src.svc_typ_cd,
                src.isn_max_dcl_vlu_a,
                src.apv_sts_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.cus_cls_typ_cd, src.cus_cls_typ_cd)     AS cus_cls_typ_cd,
            COALESCE(tgt.cny_cd, src.cny_cd)                     AS cny_cd,
            COALESCE(tgt.asy_svc_typ_cd, src.asy_svc_typ_cd)     AS asy_svc_typ_cd,
            COALESCE(tgt.svc_fea_typ_cd, src.svc_fea_typ_cd)     AS svc_fea_typ_cd,
            COALESCE(tgt.wgt_ms_unt_typ_cd, src.wgt_ms_unt_typ_cd) AS wgt_ms_unt_typ_cd,
            COALESCE(tgt.ccy_cd, src.ccy_cd)                     AS ccy_cd,
            COALESCE(tgt.ins_cri_eff_stt_dt, src.ins_cri_eff_stt_dt) AS ins_cri_eff_stt_dt,
            COALESCE(tgt.svc_typ_cd, src.svc_typ_cd)             AS svc_typ_cd,
            COALESCE(tgt.apv_sts_cd, src.apv_sts_cd)             AS apv_sts_cd
    )
    INSERT INTO merge_actions (
        table_name, action,
        cus_cls_typ_cd,
        cny_cd,
        asy_svc_typ_cd,
        svc_fea_typ_cd,
        wgt_ms_unt_typ_cd,
        ccy_cd,
        ins_cri_eff_stt_dt,
        svc_typ_cd,
        apv_sts_cd
    )
    SELECT
        'tinscri', merge_action,
        cus_cls_typ_cd,
        cny_cd,
        asy_svc_typ_cd,
        svc_fea_typ_cd,
        wgt_ms_unt_typ_cd,
        ccy_cd,
        ins_cri_eff_stt_dt,
        svc_typ_cd,
        apv_sts_cd
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_insurancecriteria_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_insurancecriteria_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;