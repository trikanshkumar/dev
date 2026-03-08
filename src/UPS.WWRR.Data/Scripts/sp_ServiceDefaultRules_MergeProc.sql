CREATE OR REPLACE PROCEDURE sp_servicedefaultrules_merge_proc(
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
        cny_cd char(2),
        svc_typ_cd char(3),
        svc_dfl_unt_typ_cd char(4),
        svc_dfl_typ_cd char(4),
        mvm_drc_cd char(1),
        rec_eff_stt_dt date,
        dtr_cha_typ_cd char(3),
        apv_sts_cd char(2),
        cri_vlu_rng_lo_qy numeric(18,4)
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               cny_cd,
               svc_typ_cd,
               svc_dfl_unt_typ_cd,
               svc_dfl_typ_cd,
               svc_dfl_vlu_te,
               udt_ts,
               mvm_drc_cd,
               rec_eff_stt_dt,
               rec_eff_end_dt,
               dtr_cha_typ_cd,
               apv_sts_cd,
               cri_vlu_rng_lo_qy,
               cri_vlu_rng_hi_qy,
               load_ref_te
          FROM tsvcdfl_stg
    ),
    cc_merge AS (
        MERGE INTO tsvcdfl AS tgt
        USING src_dedup AS src
        ON (
            tgt.cny_cd             = src.cny_cd AND
            tgt.svc_typ_cd         = src.svc_typ_cd AND
            tgt.svc_dfl_unt_typ_cd = src.svc_dfl_unt_typ_cd AND
            tgt.svc_dfl_typ_cd     = src.svc_dfl_typ_cd AND
            tgt.mvm_drc_cd         = src.mvm_drc_cd AND
            tgt.rec_eff_stt_dt     = src.rec_eff_stt_dt AND
            tgt.dtr_cha_typ_cd     = src.dtr_cha_typ_cd AND
            tgt.apv_sts_cd         = src.apv_sts_cd AND
            tgt.cri_vlu_rng_lo_qy  = src.cri_vlu_rng_lo_qy
        )
        WHEN MATCHED AND (
            tgt.svc_dfl_vlu_te  IS DISTINCT FROM src.svc_dfl_vlu_te OR
            tgt.udt_ts          IS DISTINCT FROM src.udt_ts OR
            tgt.rec_eff_end_dt  IS DISTINCT FROM src.rec_eff_end_dt OR
            tgt.cri_vlu_rng_hi_qy IS DISTINCT FROM src.cri_vlu_rng_hi_qy 
        ) THEN
            UPDATE SET
                svc_dfl_vlu_te   = src.svc_dfl_vlu_te,
                udt_ts           = src.udt_ts,
                rec_eff_end_dt   = src.rec_eff_end_dt,
                cri_vlu_rng_hi_qy= src.cri_vlu_rng_hi_qy,
                load_ref_te      = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                cny_cd,
                svc_typ_cd,
                svc_dfl_unt_typ_cd,
                svc_dfl_typ_cd,
                svc_dfl_vlu_te,
                udt_ts,
                mvm_drc_cd,
                rec_eff_stt_dt,
                rec_eff_end_dt,
                dtr_cha_typ_cd,
                apv_sts_cd,
                cri_vlu_rng_lo_qy,
                cri_vlu_rng_hi_qy,
                load_ref_te
            ) VALUES (
                src.cny_cd,
                src.svc_typ_cd,
                src.svc_dfl_unt_typ_cd,
                src.svc_dfl_typ_cd,
                src.svc_dfl_vlu_te,
                src.udt_ts,
                src.mvm_drc_cd,
                src.rec_eff_stt_dt,
                src.rec_eff_end_dt,
                src.dtr_cha_typ_cd,
                src.apv_sts_cd,
                src.cri_vlu_rng_lo_qy,
                src.cri_vlu_rng_hi_qy,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.cny_cd, src.cny_cd)                               AS cny_cd,
            COALESCE(tgt.svc_typ_cd, src.svc_typ_cd)                       AS svc_typ_cd,
            COALESCE(tgt.svc_dfl_unt_typ_cd, src.svc_dfl_unt_typ_cd)       AS svc_dfl_unt_typ_cd,
            COALESCE(tgt.svc_dfl_typ_cd, src.svc_dfl_typ_cd)               AS svc_dfl_typ_cd,
            COALESCE(tgt.mvm_drc_cd, src.mvm_drc_cd)                       AS mvm_drc_cd,
            COALESCE(tgt.rec_eff_stt_dt, src.rec_eff_stt_dt)               AS rec_eff_stt_dt,
            COALESCE(tgt.dtr_cha_typ_cd, src.dtr_cha_typ_cd)               AS dtr_cha_typ_cd,
            COALESCE(tgt.apv_sts_cd, src.apv_sts_cd)                       AS apv_sts_cd,
            COALESCE(tgt.cri_vlu_rng_lo_qy, src.cri_vlu_rng_lo_qy)         AS cri_vlu_rng_lo_qy
    )
    INSERT INTO merge_actions (
        table_name,
        action,
        cny_cd,
        svc_typ_cd,
        svc_dfl_unt_typ_cd,
        svc_dfl_typ_cd,
        mvm_drc_cd,
        rec_eff_stt_dt,
        dtr_cha_typ_cd,
        apv_sts_cd,
        cri_vlu_rng_lo_qy
    )
    SELECT
        'tsvcdfl',
        merge_action,
        cny_cd,
        svc_typ_cd,
        svc_dfl_unt_typ_cd,
        svc_dfl_typ_cd,
        mvm_drc_cd,
        rec_eff_stt_dt,
        dtr_cha_typ_cd,
        apv_sts_cd,
        cri_vlu_rng_lo_qy
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_servicedefaultrules_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_servicedefaultrules_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
