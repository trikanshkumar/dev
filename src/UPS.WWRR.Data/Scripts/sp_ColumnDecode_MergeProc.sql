CREATE OR REPLACE PROCEDURE sp_columndecode_merge_proc(
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
        table_name     text,
        action         text,
        tbl_clu_na     char(15),
        tbl_clu_vlu_te char(15),
        scr_cd_vlu_te  char(15)
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               tbl_clu_na,
               tbl_clu_vlu_te,
               scr_cd_vlu_te,
               load_ref_te
          FROM tcoldec_stg
    ),
    cc_merge AS (
        MERGE INTO tcoldec AS tgt
        USING src_dedup AS src
        ON (
            tgt.tbl_clu_na     = src.tbl_clu_na AND
            tgt.tbl_clu_vlu_te = src.tbl_clu_vlu_te AND
            tgt.scr_cd_vlu_te  = src.scr_cd_vlu_te
        )
        WHEN MATCHED THEN
            UPDATE SET
                load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                tbl_clu_na,
                tbl_clu_vlu_te,
                scr_cd_vlu_te,
                load_ref_te
            ) VALUES (
                src.tbl_clu_na,
                src.tbl_clu_vlu_te,
                src.scr_cd_vlu_te,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.tbl_clu_na,     src.tbl_clu_na)     AS tbl_clu_na,
            COALESCE(tgt.tbl_clu_vlu_te, src.tbl_clu_vlu_te) AS tbl_clu_vlu_te,
            COALESCE(tgt.scr_cd_vlu_te,  src.scr_cd_vlu_te)  AS scr_cd_vlu_te
    )
    INSERT INTO merge_actions (
        table_name, action,
        tbl_clu_na, tbl_clu_vlu_te, scr_cd_vlu_te
    )
    SELECT
        'tcoldec', merge_action,
        tbl_clu_na, tbl_clu_vlu_te, scr_cd_vlu_te
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_columndecode_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_columndecode_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
