CREATE OR REPLACE PROCEDURE sp_templateaccessorialrules_merge_proc(
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
        cny_ra_sei_rl_cd char(2),
        svc_typ_cd char(3),
        spm_lin_cd char(3),
        dtr_cri_sts_cd char(2),
        dtr_cri_eff_dt date,
        dtr_cri_end_dt date,
        chg_ccl_rul_cd char(2),
        spm_chg_rfd_elg_ir char(1),
        spm_typ_cd char(2),
        inf_xmp_ir char(1)
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               gpn_xpt_cny_cd,
               gpn_ipt_cny_cd,
               cny_ra_sei_rl_cd,
               svc_typ_cd,
               spm_lin_cd,
               dtr_cri_sts_cd,
               dtr_cri_eff_dt,
               dtr_cri_end_dt,
               chg_ccl_rul_cd,
               spm_chg_rfd_elg_ir,
               spm_typ_cd,
               inf_xmp_ir,
               load_ref_te
          FROM tspmycd_stg
    ),
    cc_merge AS (
        MERGE INTO tspmycd AS tgt
        USING src_dedup AS src
        ON (
            tgt.gpn_xpt_cny_cd     = src.gpn_xpt_cny_cd AND 
            tgt.gpn_ipt_cny_cd     = src.gpn_ipt_cny_cd AND 
            tgt.cny_ra_sei_rl_cd   = src.cny_ra_sei_rl_cd AND 
            tgt.svc_typ_cd         = src.svc_typ_cd AND 
            tgt.spm_lin_cd         = src.spm_lin_cd AND 
            tgt.dtr_cri_sts_cd     = src.dtr_cri_sts_cd AND 
            tgt.dtr_cri_eff_dt     = src.dtr_cri_eff_dt
        )
        WHEN MATCHED AND (
            tgt.dtr_cri_end_dt     IS DISTINCT FROM src.dtr_cri_end_dt AND
            tgt.chg_ccl_rul_cd     IS DISTINCT FROM src.chg_ccl_rul_cd AND
            tgt.spm_chg_rfd_elg_ir IS DISTINCT FROM src.spm_chg_rfd_elg_ir AND
            tgt.spm_typ_cd         IS DISTINCT FROM src.spm_typ_cd AND
            tgt.inf_xmp_ir         IS DISTINCT FROM src.inf_xmp_ir
        ) THEN UPDATE SET
            dtr_cri_end_dt     = src.dtr_cri_end_dt,
            chg_ccl_rul_cd     = src.chg_ccl_rul_cd,
            spm_chg_rfd_elg_ir = src.spm_chg_rfd_elg_ir,
            spm_typ_cd         = src.spm_typ_cd,
            inf_xmp_ir         = src.inf_xmp_ir
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                gpn_xpt_cny_cd,
                gpn_ipt_cny_cd,
                cny_ra_sei_rl_cd,
                svc_typ_cd,
                spm_lin_cd,
                dtr_cri_sts_cd,
                dtr_cri_eff_dt,
                dtr_cri_end_dt,
                chg_ccl_rul_cd,
                spm_chg_rfd_elg_ir,
                spm_typ_cd,
                inf_xmp_ir,
                load_ref_te
            ) VALUES (
                src.gpn_xpt_cny_cd,
                src.gpn_ipt_cny_cd,
                src.cny_ra_sei_rl_cd,
                src.svc_typ_cd,
                src.spm_lin_cd,
                src.dtr_cri_sts_cd,
                src.dtr_cri_eff_dt,
                src.dtr_cri_end_dt,
                src.chg_ccl_rul_cd,
                src.spm_chg_rfd_elg_ir,
                src.spm_typ_cd,
                src.inf_xmp_ir,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.gpn_xpt_cny_cd,     src.gpn_xpt_cny_cd)     AS gpn_xpt_cny_cd,
            COALESCE(tgt.gpn_ipt_cny_cd,     src.gpn_ipt_cny_cd)     AS gpn_ipt_cny_cd,
            COALESCE(tgt.cny_ra_sei_rl_cd,   src.cny_ra_sei_rl_cd)   AS cny_ra_sei_rl_cd,
            COALESCE(tgt.svc_typ_cd,         src.svc_typ_cd)         AS svc_typ_cd,
            COALESCE(tgt.spm_lin_cd,         src.spm_lin_cd)         AS spm_lin_cd,
            COALESCE(tgt.dtr_cri_sts_cd,     src.dtr_cri_sts_cd)     AS dtr_cri_sts_cd,
            COALESCE(tgt.dtr_cri_eff_dt,     src.dtr_cri_eff_dt)     AS dtr_cri_eff_dt,
            COALESCE(tgt.dtr_cri_end_dt,     src.dtr_cri_end_dt)     AS dtr_cri_end_dt,
            COALESCE(tgt.chg_ccl_rul_cd,     src.chg_ccl_rul_cd)     AS chg_ccl_rul_cd,
            COALESCE(tgt.spm_chg_rfd_elg_ir, src.spm_chg_rfd_elg_ir) AS spm_chg_rfd_elg_ir,
            COALESCE(tgt.spm_typ_cd,         src.spm_typ_cd)         AS spm_typ_cd,
            COALESCE(tgt.inf_xmp_ir,         src.inf_xmp_ir)         AS inf_xmp_ir
)
    INSERT INTO merge_actions (
        table_name, 
        action,
        gpn_xpt_cny_cd,
        gpn_ipt_cny_cd,
        cny_ra_sei_rl_cd,
        svc_typ_cd,
        spm_lin_cd,
        dtr_cri_sts_cd,
        dtr_cri_eff_dt,
        dtr_cri_end_dt,
        chg_ccl_rul_cd,
        spm_chg_rfd_elg_ir,
        spm_typ_cd,
        inf_xmp_ir
    )
    SELECT 
        'tspmycd', 
        merge_action,
        gpn_xpt_cny_cd,
        gpn_ipt_cny_cd,
        cny_ra_sei_rl_cd,
        svc_typ_cd,
        spm_lin_cd,
        dtr_cri_sts_cd,
        dtr_cri_eff_dt,
        dtr_cri_end_dt,
        chg_ccl_rul_cd,
        spm_chg_rfd_elg_ir,
        spm_typ_cd,
        inf_xmp_ir
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_templateaccessorialrules_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_templateaccessorialrules_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
