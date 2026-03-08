CREATE OR REPLACE PROCEDURE sp_validacquisitionmethod_merge_proc(
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
        svc_typ_cd char(3),
        pkg_acq_mth_typ_cd char(3),
        apl_ra_typ_cd char(1),
        tbl_row_eff_dt date,
        tbl_row_exp_dt date,
        apv_sts_cd char(2)
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               gpn_xpt_cny_cd,
               gpn_ipt_cny_cd,
               svc_typ_cd,
               pkg_acq_mth_typ_cd,
               apl_ra_typ_cd,
               tbl_row_eff_dt,
               tbl_row_exp_dt,
               apv_sts_cd,
               load_ref_te
          FROM tvpaqmt_stg
    ),
    cc_merge AS (
        MERGE INTO tvpaqmt AS tgt
        USING src_dedup AS src
        ON (
            tgt.gpn_xpt_cny_cd     = src.gpn_xpt_cny_cd AND
            tgt.gpn_ipt_cny_cd     = src.gpn_ipt_cny_cd AND
            tgt.svc_typ_cd         = src.svc_typ_cd AND
            tgt.pkg_acq_mth_typ_cd = src.pkg_acq_mth_typ_cd AND
            tgt.apl_ra_typ_cd      = src.apl_ra_typ_cd AND
            tgt.tbl_row_eff_dt     = src.tbl_row_eff_dt
        )
        WHEN MATCHED AND (
            tgt.tbl_row_exp_dt IS DISTINCT FROM src.tbl_row_exp_dt OR
            tgt.apv_sts_cd     IS DISTINCT FROM src.apv_sts_cd 
        ) THEN UPDATE SET
            tbl_row_exp_dt = src.tbl_row_exp_dt,
            apv_sts_cd     = src.apv_sts_cd,
            load_ref_te    = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                gpn_xpt_cny_cd,
                gpn_ipt_cny_cd,
                svc_typ_cd,
                pkg_acq_mth_typ_cd,
                apl_ra_typ_cd,
                tbl_row_eff_dt,
                tbl_row_exp_dt,
                apv_sts_cd,
                load_ref_te
            ) VALUES (
                src.gpn_xpt_cny_cd,
                src.gpn_ipt_cny_cd,
                src.svc_typ_cd,
                src.pkg_acq_mth_typ_cd,
                src.apl_ra_typ_cd,
                src.tbl_row_eff_dt,
                src.tbl_row_exp_dt,
                src.apv_sts_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.gpn_xpt_cny_cd,     src.gpn_xpt_cny_cd)     AS gpn_xpt_cny_cd,
            COALESCE(tgt.gpn_ipt_cny_cd,     src.gpn_ipt_cny_cd)     AS gpn_ipt_cny_cd,
            COALESCE(tgt.svc_typ_cd,         src.svc_typ_cd)         AS svc_typ_cd,
            COALESCE(tgt.pkg_acq_mth_typ_cd, src.pkg_acq_mth_typ_cd) AS pkg_acq_mth_typ_cd,
            COALESCE(tgt.apl_ra_typ_cd,      src.apl_ra_typ_cd)      AS apl_ra_typ_cd,
            COALESCE(tgt.tbl_row_eff_dt,     src.tbl_row_eff_dt)     AS tbl_row_eff_dt,
            COALESCE(tgt.tbl_row_exp_dt,     src.tbl_row_exp_dt)     AS tbl_row_exp_dt,
            COALESCE(tgt.apv_sts_cd,         src.apv_sts_cd)         AS apv_sts_cd
    )
    INSERT INTO merge_actions (
        table_name, 
        action,
        gpn_xpt_cny_cd,
        gpn_ipt_cny_cd,
        svc_typ_cd,
        pkg_acq_mth_typ_cd,
        apl_ra_typ_cd,
        tbl_row_eff_dt,
        tbl_row_exp_dt,
        apv_sts_cd
    )
    SELECT 
        'tvpaqmt',
        merge_action,
        gpn_xpt_cny_cd,
        gpn_ipt_cny_cd,
        svc_typ_cd,
        pkg_acq_mth_typ_cd,
        apl_ra_typ_cd,
        tbl_row_eff_dt,
        tbl_row_exp_dt,
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
    ErrorProcedure := 'sp_validacquisitionmethod_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_validacquisitionmethod_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
