CREATE OR REPLACE PROCEDURE sp_accessorialratingrules_merge_proc(
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
        cny_cd char(2),
        mvm_drc_typ_cd char(2),
        cus_csf_typ_cd char(2),
        asy_svc_typ_cd char(3),
        asy_svc_chg_eff_dt date,
        asy_svc_chg_end_dt date,
        apv_sts_cd char(2)
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               cny_cd,
               mvm_drc_typ_cd,
               cus_csf_typ_cd,
               asy_svc_typ_cd,
               asy_svc_chg_eff_dt,
               asy_svc_chg_end_dt,
               asy_svc_chg_typ_cd,
               ctl_vlu_1_te,
               ctl_vlu_2_te,
               ctl_vlu_3_te,
               ctl_vlu_4_te,
               ctl_vlu_5_te,
               ctl_vlu_6_te,
               ctl_vlu_dsc_te,
               apv_sts_cd,
               ra_typ_cd_ary_te,
               load_ref_te
          FROM tcnyasy_stg
    ),
    cc_merge AS (
        MERGE INTO tcnyasy AS tgt
        USING src_dedup AS src
        ON (
            tgt.cny_cd             = src.cny_cd AND
            tgt.mvm_drc_typ_cd     = src.mvm_drc_typ_cd AND
            tgt.cus_csf_typ_cd     = src.cus_csf_typ_cd AND
            tgt.asy_svc_typ_cd     = src.asy_svc_typ_cd AND
            tgt.asy_svc_chg_eff_dt = src.asy_svc_chg_eff_dt AND
            tgt.asy_svc_chg_end_dt = src.asy_svc_chg_end_dt AND
            tgt.apv_sts_cd         = src.apv_sts_cd
        )
        WHEN MATCHED AND (
            tgt.asy_svc_chg_typ_cd IS DISTINCT FROM src.asy_svc_chg_typ_cd OR
            tgt.ctl_vlu_1_te       IS DISTINCT FROM src.ctl_vlu_1_te OR
            tgt.ctl_vlu_2_te       IS DISTINCT FROM src.ctl_vlu_2_te OR
            tgt.ctl_vlu_3_te       IS DISTINCT FROM src.ctl_vlu_3_te OR
            tgt.ctl_vlu_4_te       IS DISTINCT FROM src.ctl_vlu_4_te OR
            tgt.ctl_vlu_5_te       IS DISTINCT FROM src.ctl_vlu_5_te OR
            tgt.ctl_vlu_6_te       IS DISTINCT FROM src.ctl_vlu_6_te OR
            tgt.ctl_vlu_dsc_te     IS DISTINCT FROM src.ctl_vlu_dsc_te OR
            tgt.ra_typ_cd_ary_te   IS DISTINCT FROM src.ra_typ_cd_ary_te OR
            tgt.load_ref_te        IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            asy_svc_chg_typ_cd = src.asy_svc_chg_typ_cd,
            ctl_vlu_1_te       = src.ctl_vlu_1_te,
            ctl_vlu_2_te       = src.ctl_vlu_2_te,
            ctl_vlu_3_te       = src.ctl_vlu_3_te,
            ctl_vlu_4_te       = src.ctl_vlu_4_te,
            ctl_vlu_5_te       = src.ctl_vlu_5_te,
            ctl_vlu_6_te       = src.ctl_vlu_6_te,
            ctl_vlu_dsc_te     = src.ctl_vlu_dsc_te,
            ra_typ_cd_ary_te   = src.ra_typ_cd_ary_te,
            load_ref_te        = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                cny_cd,
                mvm_drc_typ_cd,
                cus_csf_typ_cd,
                asy_svc_typ_cd,
                asy_svc_chg_eff_dt,
                asy_svc_chg_end_dt,
                asy_svc_chg_typ_cd,
                ctl_vlu_1_te,
                ctl_vlu_2_te,
                ctl_vlu_3_te,
                ctl_vlu_4_te,
                ctl_vlu_5_te,
                ctl_vlu_6_te,
                ctl_vlu_dsc_te,
                apv_sts_cd,
                ra_typ_cd_ary_te,
                load_ref_te
            ) VALUES (
                src.cny_cd,
                src.mvm_drc_typ_cd,
                src.cus_csf_typ_cd,
                src.asy_svc_typ_cd,
                src.asy_svc_chg_eff_dt,
                src.asy_svc_chg_end_dt,
                src.asy_svc_chg_typ_cd,
                src.ctl_vlu_1_te,
                src.ctl_vlu_2_te,
                src.ctl_vlu_3_te,
                src.ctl_vlu_4_te,
                src.ctl_vlu_5_te,
                src.ctl_vlu_6_te,
                src.ctl_vlu_dsc_te,
                src.apv_sts_cd,
                src.ra_typ_cd_ary_te,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.cny_cd,             src.cny_cd)             AS cny_cd,
            COALESCE(tgt.mvm_drc_typ_cd,     src.mvm_drc_typ_cd)     AS mvm_drc_typ_cd,
            COALESCE(tgt.cus_csf_typ_cd,     src.cus_csf_typ_cd)     AS cus_csf_typ_cd,
            COALESCE(tgt.asy_svc_typ_cd,     src.asy_svc_typ_cd)     AS asy_svc_typ_cd,
            COALESCE(tgt.asy_svc_chg_eff_dt, src.asy_svc_chg_eff_dt) AS asy_svc_chg_eff_dt,
            COALESCE(tgt.asy_svc_chg_end_dt, src.asy_svc_chg_end_dt) AS asy_svc_chg_end_dt,
            COALESCE(tgt.apv_sts_cd,         src.apv_sts_cd)         AS apv_sts_cd
    )
    INSERT INTO merge_actions (
        table_name, action,
        cny_cd, mvm_drc_typ_cd, cus_csf_typ_cd, asy_svc_typ_cd, asy_svc_chg_eff_dt, asy_svc_chg_end_dt, apv_sts_cd
    )
    SELECT 'tcnyasy', merge_action,
           cny_cd, mvm_drc_typ_cd, cus_csf_typ_cd, asy_svc_typ_cd, asy_svc_chg_eff_dt, asy_svc_chg_end_dt, apv_sts_cd
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_accessorialratingrules_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_accessorialratingrules_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;