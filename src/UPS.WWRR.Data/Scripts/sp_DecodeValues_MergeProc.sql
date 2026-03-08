CREATE OR REPLACE PROCEDURE sp_decodevalues_merge_proc(
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
        fld_na varchar(30),
        typ_cd_fld_vlu_cd char(10),
        rec_eff_stt_dt date
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               fld_na,
               typ_cd_fld_vlu_cd,
               typ_cd_fld_dsc_te,
               rec_eff_stt_dt,
               rec_eff_end_dt,
               load_ref_te
          FROM tdecode_stg
    ),
    cc_merge AS (
        MERGE INTO tdecode AS tgt
        USING src_dedup AS src
        ON (
            tgt.fld_na            = src.fld_na AND
            tgt.typ_cd_fld_vlu_cd = src.typ_cd_fld_vlu_cd AND
            tgt.rec_eff_stt_dt    = src.rec_eff_stt_dt
        )
        WHEN MATCHED AND (
            tgt.typ_cd_fld_dsc_te IS DISTINCT FROM src.typ_cd_fld_dsc_te OR
            tgt.rec_eff_end_dt IS DISTINCT FROM src.rec_eff_end_dt 
        ) THEN UPDATE SET
            typ_cd_fld_dsc_te = src.typ_cd_fld_dsc_te,
            rec_eff_end_dt = src.rec_eff_end_dt,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                fld_na,
                typ_cd_fld_vlu_cd,
                typ_cd_fld_dsc_te,
                rec_eff_stt_dt,
                rec_eff_end_dt,
                load_ref_te
            ) VALUES (
                src.fld_na,
                src.typ_cd_fld_vlu_cd,
                src.typ_cd_fld_dsc_te,
                src.rec_eff_stt_dt,
                src.rec_eff_end_dt,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.fld_na, src.fld_na) AS fld_na,
            COALESCE(tgt.typ_cd_fld_vlu_cd, src.typ_cd_fld_vlu_cd) AS typ_cd_fld_vlu_cd,
            COALESCE(tgt.rec_eff_stt_dt, src.rec_eff_stt_dt) AS rec_eff_stt_dt
    )
    INSERT INTO merge_actions (
        table_name, action,
        fld_na, typ_cd_fld_vlu_cd, rec_eff_stt_dt
    )
    SELECT 'tdecode', merge_action,
           fld_na, typ_cd_fld_vlu_cd, rec_eff_stt_dt
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_decodevalues_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_decodevalues_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
