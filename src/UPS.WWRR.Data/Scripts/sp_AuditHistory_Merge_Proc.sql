CREATE OR REPLACE PROCEDURE sp_audithistory_merge_proc(
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
        aud_typ_cd text,
        aud_acn_cd text,
        aud_trs_nr text,
        aud_trs_typ_cd text,
        rec_crt_ts timestamp,
        rec_crt_usr_nr text,
        dat_tms_snd_ts timestamp,
        aud_rmk_te text,
        aud_hdr_udt_qy decimal,
        aud_dtl_udt_qy decimal,
        aud_rpt_rsl_cd text
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               aud_typ_cd,
               aud_acn_cd,
               aud_trs_nr,
               aud_trs_typ_cd,
               rec_crt_ts,
               rec_crt_usr_nr,
               dat_tms_snd_ts,
               aud_rmk_te,
               aud_hdr_udt_qy,
               aud_dtl_udt_qy,
               aud_rpt_rsl_cd,
               load_ref_te
          FROM tauhist_stg
    ),
    cc_merge AS (
        MERGE INTO tauhist AS tgt
        USING src_dedup AS src
        ON (
            tgt.aud_typ_cd     = src.aud_typ_cd AND
            tgt.aud_acn_cd     = src.aud_acn_cd AND
            tgt.aud_trs_nr     = src.aud_trs_nr AND
            tgt.aud_trs_typ_cd = src.aud_trs_typ_cd AND
            tgt.rec_crt_ts     = src.rec_crt_ts
        )
        WHEN MATCHED AND (
            tgt.rec_crt_usr_nr IS DISTINCT FROM src.rec_crt_usr_nr OR
            tgt.dat_tms_snd_ts IS DISTINCT FROM src.dat_tms_snd_ts OR
            tgt.aud_rmk_te     IS DISTINCT FROM src.aud_rmk_te OR
            tgt.aud_hdr_udt_qy IS DISTINCT FROM src.aud_hdr_udt_qy OR
            tgt.aud_dtl_udt_qy IS DISTINCT FROM src.aud_dtl_udt_qy OR
            tgt.aud_rpt_rsl_cd IS DISTINCT FROM src.aud_rpt_rsl_cd
        )
            THEN UPDATE SET
                rec_crt_usr_nr = src.rec_crt_usr_nr,
                dat_tms_snd_ts = src.dat_tms_snd_ts,
                aud_rmk_te     = src.aud_rmk_te,
                aud_hdr_udt_qy = src.aud_hdr_udt_qy,
                aud_dtl_udt_qy = src.aud_dtl_udt_qy,
                aud_rpt_rsl_cd = src.aud_rpt_rsl_cd,
                load_ref_te    = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
               aud_typ_cd,
               aud_acn_cd,
               aud_trs_nr,
               aud_trs_typ_cd,
               rec_crt_ts,
               rec_crt_usr_nr,
               dat_tms_snd_ts,
               aud_rmk_te,
               aud_hdr_udt_qy,
               aud_dtl_udt_qy,
               aud_rpt_rsl_cd,
               load_ref_te
            ) VALUES (
               src.aud_typ_cd,
               src.aud_acn_cd,
               src.aud_trs_nr,
               src.aud_trs_typ_cd,
               src.rec_crt_ts,
               src.rec_crt_usr_nr,
               src.dat_tms_snd_ts,
               src.aud_rmk_te,
               src.aud_hdr_udt_qy,
               src.aud_dtl_udt_qy,
               src.aud_rpt_rsl_cd,
               src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(), 
            COALESCE(tgt.aud_typ_cd,     src.aud_typ_cd)     AS aud_typ_cd,
            COALESCE(tgt.aud_acn_cd,     src.aud_acn_cd)     AS aud_acn_cd,
            COALESCE(tgt.aud_trs_nr,     src.aud_trs_nr)     AS aud_trs_nr,
            COALESCE(tgt.aud_trs_typ_cd, src.aud_trs_typ_cd) AS aud_trs_typ_cd,
            COALESCE(tgt.rec_crt_ts,     src.rec_crt_ts)     AS rec_crt_ts,
            COALESCE(tgt.rec_crt_usr_nr, src.rec_crt_usr_nr) AS rec_crt_usr_nr,
            COALESCE(tgt.dat_tms_snd_ts, src.dat_tms_snd_ts) AS dat_tms_snd_ts,
            COALESCE(tgt.aud_rmk_te,     src.aud_rmk_te)     AS aud_rmk_te,
            COALESCE(tgt.aud_hdr_udt_qy, src.aud_hdr_udt_qy) AS aud_hdr_udt_qy,
            COALESCE(tgt.aud_dtl_udt_qy, src.aud_dtl_udt_qy) AS aud_dtl_udt_qy,
            COALESCE(tgt.aud_rpt_rsl_cd, src.aud_rpt_rsl_cd) AS aud_rpt_rsl_cd
    )
    INSERT INTO merge_actions (
        table_name, action,
        aud_typ_cd, aud_acn_cd, aud_trs_nr, aud_trs_typ_cd, rec_crt_ts, rec_crt_usr_nr, dat_tms_snd_ts, aud_rmk_te, aud_hdr_udt_qy, aud_dtl_udt_qy, aud_rpt_rsl_cd
    )
    SELECT
        'tauhist', merge_action,
        aud_typ_cd, aud_acn_cd, aud_trs_nr, aud_trs_typ_cd, rec_crt_ts, rec_crt_usr_nr, dat_tms_snd_ts, aud_rmk_te, aud_hdr_udt_qy, aud_dtl_udt_qy, aud_rpt_rsl_cd
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_audithistory_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_audithistory_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
