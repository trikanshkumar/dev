CREATE OR REPLACE PROCEDURE sp_publishedletterthreshold_merge_proc(
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
        cny_cd text,
        svc_typ_cd text,
        pkg_cha_typ_cd text,
        wgt_ms_unt_typ_cd text,
        pce_max_alw_wgt_qy numeric(9,2),
        trh_max_wgt_qy numeric(9,2),
        sn_tln_wgt_qy numeric(9,2),
        apv_sts_cd text,
        rec_eff_stt_dt date,
        rec_eff_end_dt date
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               cny_cd,
               svc_typ_cd,
               pkg_cha_typ_cd,
               wgt_ms_unt_typ_cd,
               pce_max_alw_wgt_qy,
               trh_max_wgt_qy,
               sn_tln_wgt_qy,
               apv_sts_cd,
               rec_eff_stt_dt,
               rec_eff_end_dt,
               load_ref_te
          FROM twgttrh_stg
    ),
    cc_merge AS (
        MERGE INTO twgttrh AS tgt
        USING src_dedup AS src
        ON (
            tgt.cny_cd            = src.cny_cd AND
            tgt.svc_typ_cd        = src.svc_typ_cd AND
            tgt.pkg_cha_typ_cd    = src.pkg_cha_typ_cd AND
            tgt.wgt_ms_unt_typ_cd = src.wgt_ms_unt_typ_cd AND
            tgt.apv_sts_cd        = src.apv_sts_cd AND
            tgt.rec_eff_stt_dt    = src.rec_eff_stt_dt
        )
        WHEN MATCHED AND (
            tgt.pce_max_alw_wgt_qy IS DISTINCT FROM src.pce_max_alw_wgt_qy OR
            tgt.trh_max_wgt_qy     IS DISTINCT FROM src.trh_max_wgt_qy OR
            tgt.sn_tln_wgt_qy      IS DISTINCT FROM src.sn_tln_wgt_qy OR
            tgt.rec_eff_end_dt     IS DISTINCT FROM src.rec_eff_end_dt
        ) THEN UPDATE SET
            load_ref_te        = src.load_ref_te,
            pce_max_alw_wgt_qy = src.pce_max_alw_wgt_qy,
            trh_max_wgt_qy     = src.trh_max_wgt_qy,
            sn_tln_wgt_qy      = src.sn_tln_wgt_qy,
            rec_eff_end_dt     = src.rec_eff_end_dt
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                cny_cd,
                svc_typ_cd,
                pkg_cha_typ_cd,
                wgt_ms_unt_typ_cd,
                pce_max_alw_wgt_qy,
                trh_max_wgt_qy,
                sn_tln_wgt_qy,
                apv_sts_cd,
                rec_eff_stt_dt,
                rec_eff_end_dt,
                load_ref_te
            ) VALUES (
                src.cny_cd,
                src.svc_typ_cd,
                src.pkg_cha_typ_cd,
                src.wgt_ms_unt_typ_cd,
                src.pce_max_alw_wgt_qy,
                src.trh_max_wgt_qy,
                src.sn_tln_wgt_qy,
                src.apv_sts_cd,
                src.rec_eff_stt_dt,
                src.rec_eff_end_dt,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.cny_cd,            src.cny_cd)            AS cny_cd,
            COALESCE(tgt.svc_typ_cd,        src.svc_typ_cd)        AS svc_typ_cd,
            COALESCE(tgt.pkg_cha_typ_cd,    src.pkg_cha_typ_cd)    AS pkg_cha_typ_cd,
            COALESCE(tgt.wgt_ms_unt_typ_cd, src.wgt_ms_unt_typ_cd) AS wgt_ms_unt_typ_cd,
            COALESCE(tgt.pce_max_alw_wgt_qy, src.pce_max_alw_wgt_qy) AS pce_max_alw_wgt_qy,
            COALESCE(tgt.trh_max_wgt_qy,    src.trh_max_wgt_qy)    AS trh_max_wgt_qy,
            COALESCE(tgt.sn_tln_wgt_qy,     src.sn_tln_wgt_qy)     AS sn_tln_wgt_qy,
            COALESCE(tgt.apv_sts_cd,        src.apv_sts_cd)        AS apv_sts_cd,
            COALESCE(tgt.rec_eff_stt_dt,    src.rec_eff_stt_dt)    AS rec_eff_stt_dt,
            COALESCE(tgt.rec_eff_end_dt,    src.rec_eff_end_dt)    AS rec_eff_end_dt
    )
    INSERT INTO merge_actions(
        table_name, action,
        cny_cd, svc_typ_cd, pkg_cha_typ_cd, wgt_ms_unt_typ_cd, pce_max_alw_wgt_qy, trh_max_wgt_qy, sn_tln_wgt_qy, apv_sts_cd, rec_eff_stt_dt, rec_eff_end_dt
    )
    SELECT 'twgttrh', merge_action,
           cny_cd, svc_typ_cd, pkg_cha_typ_cd, wgt_ms_unt_typ_cd, pce_max_alw_wgt_qy, trh_max_wgt_qy, sn_tln_wgt_qy, apv_sts_cd, rec_eff_stt_dt, rec_eff_end_dt
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_publishedletterthreshold_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_publishedletterthreshold_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
