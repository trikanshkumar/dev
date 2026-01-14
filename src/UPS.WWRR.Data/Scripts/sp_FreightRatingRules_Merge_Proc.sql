CREATE OR REPLACE PROCEDURE sp_freightratingrules_merge_proc(
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
        rtg_cny_cd char(2),
        cny_ra_sei_rl_cd char(2),
        cus_csf_typ_cd char(2),
        svc_typ_cd char(3),
        mps_pkg_ir char(2),
        tbl_row_eff_dt date,
        tbl_row_end_dt date,
        ctl_vlu_1_te char(6),
        ctl_vlu_2_te char(6),
        ctl_vlu_3_te char(6),
        ctl_vlu_4_te char(6),
        ctl_vlu_5_te char(6),
        ctl_vlu_6_te char(6),
        ctl_vlu_dsc_te char(35),
        apv_sts_cd char(2),
        ra_typ_cd_ary_te char(20)
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               rtg_cny_cd,
               cny_ra_sei_rl_cd,
               cus_csf_typ_cd,
               svc_typ_cd,
               mps_pkg_ir,
               tbl_row_eff_dt,
               tbl_row_end_dt,
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
          FROM tratrul_stg
    ),
    cc_merge AS (
        MERGE INTO tratrul AS tgt
        USING src_dedup AS src
        ON (
            tgt.rtg_cny_cd       = src.rtg_cny_cd AND
            tgt.cny_ra_sei_rl_cd = src.cny_ra_sei_rl_cd AND
            tgt.cus_csf_typ_cd   = src.cus_csf_typ_cd AND
            tgt.svc_typ_cd       = src.svc_typ_cd AND
            tgt.mps_pkg_ir       = src.mps_pkg_ir AND
            tgt.tbl_row_eff_dt   = src.tbl_row_eff_dt AND
            tgt.tbl_row_end_dt   = src.tbl_row_end_dt AND
            tgt.apv_sts_cd       = src.apv_sts_cd
        )
        WHEN MATCHED AND (
            tgt.ctl_vlu_1_te     IS DISTINCT FROM src.ctl_vlu_1_te OR
            tgt.ctl_vlu_2_te     IS DISTINCT FROM src.ctl_vlu_2_te OR
            tgt.ctl_vlu_3_te     IS DISTINCT FROM src.ctl_vlu_3_te OR
            tgt.ctl_vlu_4_te     IS DISTINCT FROM src.ctl_vlu_4_te OR
            tgt.ctl_vlu_5_te     IS DISTINCT FROM src.ctl_vlu_5_te OR
            tgt.ctl_vlu_6_te     IS DISTINCT FROM src.ctl_vlu_6_te OR
            tgt.ctl_vlu_dsc_te   IS DISTINCT FROM src.ctl_vlu_dsc_te OR
            tgt.ra_typ_cd_ary_te IS DISTINCT FROM src.ra_typ_cd_ary_te
        ) THEN UPDATE SET
            ctl_vlu_1_te     = src.ctl_vlu_1_te,
            ctl_vlu_2_te     = src.ctl_vlu_2_te,
            ctl_vlu_3_te     = src.ctl_vlu_3_te,
            ctl_vlu_4_te     = src.ctl_vlu_4_te,
            ctl_vlu_5_te     = src.ctl_vlu_5_te,
            ctl_vlu_6_te     = src.ctl_vlu_6_te,
            ctl_vlu_dsc_te   = src.ctl_vlu_dsc_te,
            ra_typ_cd_ary_te = src.ra_typ_cd_ary_te,
            load_ref_te      = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                rtg_cny_cd,
                cny_ra_sei_rl_cd,
                cus_csf_typ_cd,
                svc_typ_cd,
                mps_pkg_ir,
                tbl_row_eff_dt,
                tbl_row_end_dt,
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
                src.rtg_cny_cd,
                src.cny_ra_sei_rl_cd,
                src.cus_csf_typ_cd,
                src.svc_typ_cd,
                src.mps_pkg_ir,
                src.tbl_row_eff_dt,
                src.tbl_row_end_dt,
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
            COALESCE(tgt.rtg_cny_cd,       src.rtg_cny_cd)       AS rtg_cny_cd,
            COALESCE(tgt.cny_ra_sei_rl_cd, src.cny_ra_sei_rl_cd) AS cny_ra_sei_rl_cd,
            COALESCE(tgt.cus_csf_typ_cd,   src.cus_csf_typ_cd)   AS cus_csf_typ_cd,
            COALESCE(tgt.svc_typ_cd,       src.svc_typ_cd)       AS svc_typ_cd,
            COALESCE(tgt.mps_pkg_ir,       src.mps_pkg_ir)       AS mps_pkg_ir,
            COALESCE(tgt.tbl_row_eff_dt,   src.tbl_row_eff_dt)   AS tbl_row_eff_dt,
            COALESCE(tgt.tbl_row_end_dt,   src.tbl_row_end_dt)   AS tbl_row_end_dt,
            COALESCE(tgt.ctl_vlu_1_te,     src.ctl_vlu_1_te)     AS ctl_vlu_1_te,
            COALESCE(tgt.ctl_vlu_2_te,     src.ctl_vlu_2_te)     AS ctl_vlu_2_te,
            COALESCE(tgt.ctl_vlu_3_te,     src.ctl_vlu_3_te)     AS ctl_vlu_3_te,
            COALESCE(tgt.ctl_vlu_4_te,     src.ctl_vlu_4_te)     AS ctl_vlu_4_te,
            COALESCE(tgt.ctl_vlu_5_te,     src.ctl_vlu_5_te)     AS ctl_vlu_5_te,
            COALESCE(tgt.ctl_vlu_6_te,     src.ctl_vlu_6_te)     AS ctl_vlu_6_te,
            COALESCE(tgt.ctl_vlu_dsc_te,   src.ctl_vlu_dsc_te)   AS ctl_vlu_dsc_te,
            COALESCE(tgt.apv_sts_cd,       src.apv_sts_cd)       AS apv_sts_cd,
            COALESCE(tgt.ra_typ_cd_ary_te, src.ra_typ_cd_ary_te) AS ra_typ_cd_ary_te
    )
    INSERT INTO merge_actions (
        table_name, 
        action,
        rtg_cny_cd,
        cny_ra_sei_rl_cd,
        cus_csf_typ_cd,
        svc_typ_cd,
        mps_pkg_ir,
        tbl_row_eff_dt,
        tbl_row_end_dt,
        ctl_vlu_1_te,
        ctl_vlu_2_te,
        ctl_vlu_3_te,
        ctl_vlu_4_te,
        ctl_vlu_5_te,
        ctl_vlu_6_te,
        ctl_vlu_dsc_te,
        apv_sts_cd,
        ra_typ_cd_ary_te
    )
    SELECT 
        'tratrul', 
        merge_action,
        rtg_cny_cd,
        cny_ra_sei_rl_cd,
        cus_csf_typ_cd,
        svc_typ_cd,
        mps_pkg_ir,
        tbl_row_eff_dt,
        tbl_row_end_dt,
        ctl_vlu_1_te,
        ctl_vlu_2_te,
        ctl_vlu_3_te,
        ctl_vlu_4_te,
        ctl_vlu_5_te,
        ctl_vlu_6_te,
        ctl_vlu_dsc_te,
        apv_sts_cd,
        ra_typ_cd_ary_te
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_freightratingrules_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_freightratingrules_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
