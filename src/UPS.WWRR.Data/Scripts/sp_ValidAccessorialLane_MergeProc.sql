CREATE OR REPLACE PROCEDURE sp_validaccessoriallane_merge_proc(
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
        org_cny_cd char(2),
        dtn_cny_cd char(2),
        asy_svc_typ_cd char(3),
        svc_typ_cd char(3),
        mvm_drc_cd char(1),
        gpn_unt_pir_csf_cd char(1),
        apv_sts_cd char(2),
        rec_eff_stt_dt date,
        asy_svc_alt_nmc_cd char(3),
        org_gpn_mnm_te char(4),
        dtn_gpn_mnm_te char(4)
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               org_cny_cd,
               dtn_cny_cd,
               asy_svc_typ_cd,
               svc_typ_cd,
               mvm_drc_cd,
               gpn_unt_pir_csf_cd,
               apv_sts_cd,
               rec_eff_stt_dt,
               asy_svc_alt_nmc_cd,
               svc_typ_alt_nmc_cd,
               rec_eff_end_dt,
               org_gpn_mnm_te,
               dtn_gpn_mnm_te,
               load_ref_te
          FROM tvasyln_stg
    ),
    cc_merge AS (
        MERGE INTO tvasyln AS tgt
        USING src_dedup AS src
        ON (
            tgt.org_cny_cd         = src.org_cny_cd AND
            tgt.dtn_cny_cd         = src.dtn_cny_cd AND
            tgt.asy_svc_typ_cd     = src.asy_svc_typ_cd AND
            tgt.svc_typ_cd         = src.svc_typ_cd AND
            tgt.mvm_drc_cd         = src.mvm_drc_cd AND
            tgt.gpn_unt_pir_csf_cd = src.gpn_unt_pir_csf_cd AND
            tgt.apv_sts_cd         = src.apv_sts_cd AND
            tgt.rec_eff_stt_dt     = src.rec_eff_stt_dt AND
            tgt.asy_svc_alt_nmc_cd = src.asy_svc_alt_nmc_cd AND
            tgt.org_gpn_mnm_te     = src.org_gpn_mnm_te AND
            tgt.dtn_gpn_mnm_te     = src.dtn_gpn_mnm_te
        )
        WHEN MATCHED AND (
            tgt.svc_typ_alt_nmc_cd IS DISTINCT FROM src.svc_typ_alt_nmc_cd OR
            tgt.rec_eff_end_dt     IS DISTINCT FROM src.rec_eff_end_dt OR
            tgt.load_ref_te        IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            svc_typ_alt_nmc_cd = src.svc_typ_alt_nmc_cd,
            rec_eff_end_dt     = src.rec_eff_end_dt,
            load_ref_te        = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                org_cny_cd,
                dtn_cny_cd,
                asy_svc_typ_cd,
                svc_typ_cd,
                mvm_drc_cd,
                gpn_unt_pir_csf_cd,
                apv_sts_cd,
                rec_eff_stt_dt,
                asy_svc_alt_nmc_cd,
                svc_typ_alt_nmc_cd,
                rec_eff_end_dt,
                org_gpn_mnm_te,
                dtn_gpn_mnm_te,
                load_ref_te
            ) VALUES (
                src.org_cny_cd,
                src.dtn_cny_cd,
                src.asy_svc_typ_cd,
                src.svc_typ_cd,
                src.mvm_drc_cd,
                src.gpn_unt_pir_csf_cd,
                src.apv_sts_cd,
                src.rec_eff_stt_dt,
                src.asy_svc_alt_nmc_cd,
                src.svc_typ_alt_nmc_cd,
                src.rec_eff_end_dt,
                src.org_gpn_mnm_te,
                src.dtn_gpn_mnm_te,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.org_cny_cd,         src.org_cny_cd)         AS org_cny_cd,
            COALESCE(tgt.dtn_cny_cd,         src.dtn_cny_cd)         AS dtn_cny_cd,
            COALESCE(tgt.asy_svc_typ_cd,     src.asy_svc_typ_cd)     AS asy_svc_typ_cd,
            COALESCE(tgt.svc_typ_cd,         src.svc_typ_cd)         AS svc_typ_cd,
            COALESCE(tgt.mvm_drc_cd,         src.mvm_drc_cd)         AS mvm_drc_cd,
            COALESCE(tgt.gpn_unt_pir_csf_cd, src.gpn_unt_pir_csf_cd) AS gpn_unt_pir_csf_cd,
            COALESCE(tgt.apv_sts_cd,         src.apv_sts_cd)         AS apv_sts_cd,
            COALESCE(tgt.rec_eff_stt_dt,     src.rec_eff_stt_dt)     AS rec_eff_stt_dt,
            COALESCE(tgt.asy_svc_alt_nmc_cd, src.asy_svc_alt_nmc_cd) AS asy_svc_alt_nmc_cd,
            COALESCE(tgt.org_gpn_mnm_te,     src.org_gpn_mnm_te)     AS org_gpn_mnm_te,
            COALESCE(tgt.dtn_gpn_mnm_te,     src.dtn_gpn_mnm_te)     AS dtn_gpn_mnm_te
    )
    INSERT INTO merge_actions (
        table_name,
        action,
        org_cny_cd,
        dtn_cny_cd,
        asy_svc_typ_cd,
        svc_typ_cd,
        mvm_drc_cd,
        gpn_unt_pir_csf_cd,
        apv_sts_cd,
        rec_eff_stt_dt,
        asy_svc_alt_nmc_cd,
        org_gpn_mnm_te,
        dtn_gpn_mnm_te
    )
    SELECT
        'tvasyln',
        merge_action,
        org_cny_cd,
        dtn_cny_cd,
        asy_svc_typ_cd,
        svc_typ_cd,
        mvm_drc_cd,
        gpn_unt_pir_csf_cd,
        apv_sts_cd,
        rec_eff_stt_dt,
        asy_svc_alt_nmc_cd,
        org_gpn_mnm_te,
        dtn_gpn_mnm_te
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_validaccessoriallane_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_validaccessoriallane_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
