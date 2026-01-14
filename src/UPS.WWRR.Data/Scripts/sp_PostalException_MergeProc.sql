CREATE OR REPLACE PROCEDURE sp_postalexception_merge_proc(
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
        brl_cd char(2),
        gpn_xpt_cny_cd char(4),
        gpn_ipt_cny_cd char(4),
        rng_low_psl_cd char(12),
        rng_hi_psl_cd char(12),
        rec_eff_stt_dt date,
        apv_sts_cd char(2),
        rec_eff_end_dt date,
        pol_div_2_na varchar(50),
        rec_ins_ts timestamp(6)
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               brl_cd,
               gpn_xpt_cny_cd,
               gpn_ipt_cny_cd,
               rng_low_psl_cd,
               rng_hi_psl_cd,
               rec_eff_stt_dt,
               apv_sts_cd,
               rec_eff_end_dt,
               pol_div_2_na,
               rec_ins_ts,
               load_ref_te
          FROM tpslbur_stg
    ),
    cc_merge AS (
        MERGE INTO tpslbur AS tgt
        USING src_dedup AS src
        ON (
            tgt.brl_cd          = src.brl_cd AND
            tgt.gpn_xpt_cny_cd  = src.gpn_xpt_cny_cd AND
            tgt.gpn_ipt_cny_cd  = src.gpn_ipt_cny_cd AND
            tgt.rng_low_psl_cd  = src.rng_low_psl_cd AND
            tgt.rng_hi_psl_cd   = src.rng_hi_psl_cd AND
            tgt.rec_eff_stt_dt  = src.rec_eff_stt_dt AND
            tgt.apv_sts_cd      = src.apv_sts_cd
        )
        WHEN MATCHED AND (
            tgt.rec_eff_end_dt IS DISTINCT FROM src.rec_eff_end_dt OR
            tgt.pol_div_2_na   IS DISTINCT FROM src.pol_div_2_na OR
            tgt.rec_ins_ts     IS DISTINCT FROM src.rec_ins_ts OR
            tgt.load_ref_te    IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            rec_eff_end_dt = src.rec_eff_end_dt,
            pol_div_2_na   = src.pol_div_2_na,
            rec_ins_ts     = src.rec_ins_ts,
            load_ref_te    = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                brl_cd,
                gpn_xpt_cny_cd,
                gpn_ipt_cny_cd,
                rng_low_psl_cd,
                rng_hi_psl_cd,
                rec_eff_stt_dt,
                apv_sts_cd,
                rec_eff_end_dt,
                pol_div_2_na,
                rec_ins_ts,
                load_ref_te
            ) VALUES (
                src.brl_cd,
                src.gpn_xpt_cny_cd,
                src.gpn_ipt_cny_cd,
                src.rng_low_psl_cd,
                src.rng_hi_psl_cd,
                src.rec_eff_stt_dt,
                src.apv_sts_cd,
                src.rec_eff_end_dt,
                src.pol_div_2_na,
                src.rec_ins_ts,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.brl_cd,         src.brl_cd)         AS brl_cd,
            COALESCE(tgt.gpn_xpt_cny_cd, src.gpn_xpt_cny_cd) AS gpn_xpt_cny_cd,
            COALESCE(tgt.gpn_ipt_cny_cd, src.gpn_ipt_cny_cd) AS gpn_ipt_cny_cd,
            COALESCE(tgt.rng_low_psl_cd, src.rng_low_psl_cd) AS rng_low_psl_cd,
            COALESCE(tgt.rng_hi_psl_cd,  src.rng_hi_psl_cd)  AS rng_hi_psl_cd,
            COALESCE(tgt.rec_eff_stt_dt, src.rec_eff_stt_dt) AS rec_eff_stt_dt,
            COALESCE(tgt.apv_sts_cd,     src.apv_sts_cd)     AS apv_sts_cd,
            COALESCE(tgt.rec_eff_end_dt, src.rec_eff_end_dt) AS rec_eff_end_dt,
            COALESCE(tgt.pol_div_2_na,   src.pol_div_2_na)   AS pol_div_2_na,
            COALESCE(tgt.rec_ins_ts,     src.rec_ins_ts)     AS rec_ins_ts
    )
    INSERT INTO merge_actions(
        table_name, action,
        brl_cd, gpn_xpt_cny_cd, gpn_ipt_cny_cd, rng_low_psl_cd, rng_hi_psl_cd, rec_eff_stt_dt, apv_sts_cd,
        rec_eff_end_dt, pol_div_2_na, rec_ins_ts
    )
    SELECT 'tpslbur', merge_action,
           brl_cd, gpn_xpt_cny_cd, gpn_ipt_cny_cd, rng_low_psl_cd, rng_hi_psl_cd, rec_eff_stt_dt, apv_sts_cd,
           rec_eff_end_dt, pol_div_2_na, rec_ins_ts
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_postalexception_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_postalexception_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;