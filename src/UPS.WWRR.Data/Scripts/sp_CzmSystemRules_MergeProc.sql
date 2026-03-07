CREATE OR REPLACE PROCEDURE sp_czmsystemrules_merge_proc(
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
        cd_tbl_typ_cd text,
        cd_tbl_cd text,
        cd_tbl_stt_dt date,
        cd_tbl_end_dt date,
        apv_sts_cd text
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               cd_tbl_typ_cd,
               cd_tbl_cd,
               dco_cd_dsc_te,
               cd_tbl_stt_dt,
               cd_tbl_end_dt,
               apv_sts_cd,
               load_ref_te
          FROM tczmsys_stg
    ),
    cc_merge AS (
        MERGE INTO tczmsys AS tgt
        USING src_dedup AS src
        ON (
            tgt.cd_tbl_typ_cd  = src.cd_tbl_typ_cd AND
            tgt.cd_tbl_cd      = src.cd_tbl_cd AND
            tgt.cd_tbl_stt_dt  = src.cd_tbl_stt_dt AND
            tgt.cd_tbl_end_dt  = src.cd_tbl_end_dt AND
            tgt.apv_sts_cd     = src.apv_sts_cd
        )
        WHEN MATCHED AND (
            tgt.dco_cd_dsc_te IS DISTINCT FROM src.dco_cd_dsc_te 
        )
            THEN UPDATE SET
                dco_cd_dsc_te = src.dco_cd_dsc_te,
                load_ref_te   = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                cd_tbl_typ_cd,
                cd_tbl_cd,
                dco_cd_dsc_te,
                cd_tbl_stt_dt,
                cd_tbl_end_dt,
                apv_sts_cd,
                load_ref_te
            ) VALUES (
                src.cd_tbl_typ_cd,
                src.cd_tbl_cd,
                src.dco_cd_dsc_te,
                src.cd_tbl_stt_dt,
                src.cd_tbl_end_dt,
                src.apv_sts_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.cd_tbl_typ_cd, src.cd_tbl_typ_cd) AS cd_tbl_typ_cd,
            COALESCE(tgt.cd_tbl_cd,     src.cd_tbl_cd)     AS cd_tbl_cd,
            COALESCE(tgt.cd_tbl_stt_dt, src.cd_tbl_stt_dt) AS cd_tbl_stt_dt,
            COALESCE(tgt.cd_tbl_end_dt, src.cd_tbl_end_dt) AS cd_tbl_end_dt,
            COALESCE(tgt.apv_sts_cd,    src.apv_sts_cd)    AS apv_sts_cd
    )
    INSERT INTO merge_actions (
        table_name, action,
        cd_tbl_typ_cd, cd_tbl_cd, cd_tbl_stt_dt, cd_tbl_end_dt, apv_sts_cd
    )
    SELECT
        'tczmsys', merge_action,
        cd_tbl_typ_cd, cd_tbl_cd, cd_tbl_stt_dt, cd_tbl_end_dt, apv_sts_cd
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_czmsystemrules_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_czmsystemrules_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;