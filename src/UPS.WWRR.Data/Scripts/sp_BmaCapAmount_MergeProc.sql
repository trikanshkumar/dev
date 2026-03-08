CREATE OR REPLACE PROCEDURE sp_bmacapamount_merge_proc(
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
        gpn_cd            char(4),
        svc_typ_cd        char(3),
        rec_eff_stt_dt    date,
        svc_ra_cht_sts_cd char(2),
        rec_eff_end_dt    date
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               gpn_cd,
               svc_typ_cd,
               rec_eff_stt_dt,
               svc_ra_cht_sts_cd,
               max_ncv_pr,
               ups_ofr_pgm_cd,
               rec_eff_end_dt,
               load_ref_te
          FROM tbmavcs_stg
    ),
    cc_merge AS (
        MERGE INTO tbmavcs AS tgt
        USING src_dedup AS src
        ON (
            tgt.gpn_cd            = src.gpn_cd AND
            tgt.svc_typ_cd        = src.svc_typ_cd AND
            tgt.rec_eff_stt_dt    = src.rec_eff_stt_dt AND
            tgt.svc_ra_cht_sts_cd = src.svc_ra_cht_sts_cd
        )
        WHEN MATCHED AND (
            tgt.max_ncv_pr      IS DISTINCT FROM src.max_ncv_pr OR
            tgt.ups_ofr_pgm_cd  IS DISTINCT FROM src.ups_ofr_pgm_cd OR
            tgt.rec_eff_end_dt  IS DISTINCT FROM src.rec_eff_end_dt
        ) THEN UPDATE SET
            max_ncv_pr      = src.max_ncv_pr,
            ups_ofr_pgm_cd  = src.ups_ofr_pgm_cd,
            rec_eff_end_dt  = src.rec_eff_end_dt,
            load_ref_te     = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                gpn_cd,
                svc_typ_cd,
                rec_eff_stt_dt,
                svc_ra_cht_sts_cd,
                max_ncv_pr,
                ups_ofr_pgm_cd,
                rec_eff_end_dt,
                load_ref_te
            ) VALUES (
                src.gpn_cd,
                src.svc_typ_cd,
                src.rec_eff_stt_dt,
                src.svc_ra_cht_sts_cd,
                src.max_ncv_pr,
                src.ups_ofr_pgm_cd,
                src.rec_eff_end_dt,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.gpn_cd,            src.gpn_cd)            AS gpn_cd,
            COALESCE(tgt.svc_typ_cd,        src.svc_typ_cd)        AS svc_typ_cd,
            COALESCE(tgt.rec_eff_stt_dt,    src.rec_eff_stt_dt)    AS rec_eff_stt_dt,
            COALESCE(tgt.svc_ra_cht_sts_cd, src.svc_ra_cht_sts_cd) AS svc_ra_cht_sts_cd,
            COALESCE(tgt.rec_eff_end_dt,    src.rec_eff_end_dt)    AS rec_eff_end_dt
    )
    INSERT INTO merge_actions (
        table_name, action,
        gpn_cd, svc_typ_cd, rec_eff_stt_dt, svc_ra_cht_sts_cd, rec_eff_end_dt
    )
    SELECT
        'tbmavcs', merge_action,
        gpn_cd, svc_typ_cd, rec_eff_stt_dt, svc_ra_cht_sts_cd, rec_eff_end_dt
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_bmacapamount_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_bmacapamount_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;