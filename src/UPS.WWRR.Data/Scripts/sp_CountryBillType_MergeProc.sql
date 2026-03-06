CREATE OR REPLACE PROCEDURE sp_countrybilltype_merge_proc(
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
        mvm_drc_cd text,
        bil_ter_typ_cd text,
        cny_bil_ter_stt_dt date,
        cny_bil_ter_end_dt date,
        apv_sts_cd text
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               cny_cd,
               mvm_drc_cd,
               bil_ter_typ_cd,
               cny_bil_ter_stt_dt,
               cny_bil_ter_end_dt,
               apv_sts_cd,
               load_ref_te
          FROM tcyblty_stg
    ),
    cc_merge AS (
        MERGE INTO tcyblty AS tgt
        USING src_dedup AS src
        ON (
            tgt.cny_cd               = src.cny_cd AND
            tgt.mvm_drc_cd           = src.mvm_drc_cd AND
            tgt.bil_ter_typ_cd       = src.bil_ter_typ_cd AND
            tgt.cny_bil_ter_stt_dt   = src.cny_bil_ter_stt_dt AND
            tgt.cny_bil_ter_end_dt   = src.cny_bil_ter_end_dt AND
            tgt.apv_sts_cd           = src.apv_sts_cd
        )
        WHEN MATCHED AND (
            tgt.load_ref_te IS DISTINCT FROM src.load_ref_te
        ) THEN
            UPDATE SET
                load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                cny_cd,
                mvm_drc_cd,
                bil_ter_typ_cd,
                cny_bil_ter_stt_dt,
                cny_bil_ter_end_dt,
                apv_sts_cd,
                load_ref_te
            ) VALUES (
                src.cny_cd,
                src.mvm_drc_cd,
                src.bil_ter_typ_cd,
                src.cny_bil_ter_stt_dt,
                src.cny_bil_ter_end_dt,
                src.apv_sts_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.cny_cd,             src.cny_cd)             AS cny_cd,
            COALESCE(tgt.mvm_drc_cd,         src.mvm_drc_cd)         AS mvm_drc_cd,
            COALESCE(tgt.bil_ter_typ_cd,     src.bil_ter_typ_cd)     AS bil_ter_typ_cd,
            COALESCE(tgt.cny_bil_ter_stt_dt, src.cny_bil_ter_stt_dt) AS cny_bil_ter_stt_dt,
            COALESCE(tgt.cny_bil_ter_end_dt, src.cny_bil_ter_end_dt) AS cny_bil_ter_end_dt,
            COALESCE(tgt.apv_sts_cd,         src.apv_sts_cd)         AS apv_sts_cd
    )
    INSERT INTO merge_actions (
        table_name, action,
        cny_cd, mvm_drc_cd, bil_ter_typ_cd, cny_bil_ter_stt_dt, cny_bil_ter_end_dt, apv_sts_cd
    )
    SELECT
        'tcyblty', merge_action,
        cny_cd, mvm_drc_cd, bil_ter_typ_cd, cny_bil_ter_stt_dt, cny_bil_ter_end_dt, apv_sts_cd
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_countrybilltype_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_countrybilltype_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;