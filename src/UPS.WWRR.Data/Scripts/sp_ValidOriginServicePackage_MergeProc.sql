CREATE OR REPLACE PROCEDURE sp_validoriginservicepackage_merge_proc(
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
        svc_typ_cd char(3),
        pkg_cha_typ_cd char(3),
        apv_sts_cd char(2),
        rec_eff_stt_dt date,
        rec_eff_end_dt date
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               gpn_xpt_cny_cd,
               svc_typ_cd,
               pkg_cha_typ_cd,
               apv_sts_cd,
               rec_eff_stt_dt,
               rec_eff_end_dt,
               load_ref_te
          FROM tvsvcpk_stg
    ),
    cc_merge AS (
        MERGE INTO tvsvcpk AS tgt
        USING src_dedup AS src
        ON (
            tgt.gpn_xpt_cny_cd  = src.gpn_xpt_cny_cd AND
            tgt.svc_typ_cd     = src.svc_typ_cd AND
            tgt.pkg_cha_typ_cd = src.pkg_cha_typ_cd AND
            tgt.apv_sts_cd     = src.apv_sts_cd AND
            tgt.rec_eff_stt_dt = src.rec_eff_stt_dt
        )
        WHEN MATCHED AND (
            tgt.rec_eff_end_dt IS DISTINCT FROM src.rec_eff_end_dt 
        ) THEN UPDATE SET
            rec_eff_end_dt = src.rec_eff_end_dt,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                gpn_xpt_cny_cd,
                svc_typ_cd,
                pkg_cha_typ_cd,
                apv_sts_cd,
                rec_eff_stt_dt,
                rec_eff_end_dt,
                load_ref_te
            ) VALUES (
                src.gpn_xpt_cny_cd,
                src.svc_typ_cd,
                src.pkg_cha_typ_cd,
                src.apv_sts_cd,
                src.rec_eff_stt_dt,
                src.rec_eff_end_dt,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.gpn_xpt_cny_cd, src.gpn_xpt_cny_cd) AS gpn_xpt_cny_cd,
            COALESCE(tgt.svc_typ_cd,     src.svc_typ_cd)     AS svc_typ_cd,
            COALESCE(tgt.pkg_cha_typ_cd, src.pkg_cha_typ_cd) AS pkg_cha_typ_cd,
            COALESCE(tgt.apv_sts_cd,     src.apv_sts_cd)     AS apv_sts_cd,
            COALESCE(tgt.rec_eff_stt_dt, src.rec_eff_stt_dt) AS rec_eff_stt_dt,
            COALESCE(tgt.rec_eff_end_dt, src.rec_eff_end_dt) AS rec_eff_end_dt
    )
    INSERT INTO merge_actions (
        table_name, action,
        gpn_xpt_cny_cd, svc_typ_cd, pkg_cha_typ_cd, apv_sts_cd, rec_eff_stt_dt, rec_eff_end_dt
    )
    SELECT 'tvsvcpk', merge_action,
           gpn_xpt_cny_cd, svc_typ_cd, pkg_cha_typ_cd, apv_sts_cd, rec_eff_stt_dt, rec_eff_end_dt
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_validoriginservicepackage_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_validoriginservicepackage_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
