CREATE OR REPLACE PROCEDURE sp_informationalaccessorialcharge_merge_proc(
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
        asy_svc_typ_cd text,
        tm_prd_typ_cd text,
        ccy_cd text,
        dtr_cri_eff_dt date,
        dtr_cri_end_dt date,
        dtr_cri_vlu_a decimal,
        cus_csf_typ_cd text
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               cny_cd,
               asy_svc_typ_cd,
               tm_prd_typ_cd,
               ccy_cd,
               dtr_cri_eff_dt,
               dtr_cri_end_dt,
               dtr_cri_vlu_a,
               cus_csf_typ_cd,
               load_ref_te
          FROM tinfchg_stg
    ),
    cc_merge AS (
        MERGE INTO tinfchg AS tgt
        USING src_dedup AS src
        ON (
            tgt.cny_cd         = src.cny_cd AND
            tgt.asy_svc_typ_cd = src.asy_svc_typ_cd AND
            tgt.tm_prd_typ_cd  = src.tm_prd_typ_cd AND
            tgt.ccy_cd         = src.ccy_cd AND
            tgt.dtr_cri_eff_dt = src.dtr_cri_eff_dt AND
            tgt.cus_csf_typ_cd = src.cus_csf_typ_cd
        )
        WHEN MATCHED AND (
            tgt.dtr_cri_end_dt IS DISTINCT FROM src.dtr_cri_end_dt OR
            tgt.dtr_cri_vlu_a  IS DISTINCT FROM src.dtr_cri_vlu_a 
        )
            THEN UPDATE SET
                dtr_cri_end_dt = src.dtr_cri_end_dt,
                dtr_cri_vlu_a  = src.dtr_cri_vlu_a,
                load_ref_te    = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
               cny_cd,
               asy_svc_typ_cd,
               tm_prd_typ_cd,
               ccy_cd,
               dtr_cri_eff_dt,
               dtr_cri_end_dt,
               dtr_cri_vlu_a,
               cus_csf_typ_cd,
               load_ref_te
            ) VALUES (
                src.cny_cd,
                src.asy_svc_typ_cd,
                src.tm_prd_typ_cd,
                src.ccy_cd,
                src.dtr_cri_eff_dt,
                src.dtr_cri_end_dt,
                src.dtr_cri_vlu_a,
                src.cus_csf_typ_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(), 
            COALESCE(tgt.cny_cd,         src.cny_cd)         AS cny_cd,
            COALESCE(tgt.asy_svc_typ_cd, src.asy_svc_typ_cd) AS asy_svc_typ_cd,
            COALESCE(tgt.tm_prd_typ_cd,  src.tm_prd_typ_cd)  AS tm_prd_typ_cd,
            COALESCE(tgt.ccy_cd,         src.ccy_cd)         AS ccy_cd,
            COALESCE(tgt.dtr_cri_eff_dt, src.dtr_cri_eff_dt) AS dtr_cri_eff_dt,
            COALESCE(tgt.dtr_cri_end_dt, src.dtr_cri_end_dt) AS dtr_cri_end_dt,
            COALESCE(tgt.dtr_cri_vlu_a,  src.dtr_cri_vlu_a)  AS dtr_cri_vlu_a,
            COALESCE(tgt.cus_csf_typ_cd, src.cus_csf_typ_cd) AS cus_csf_typ_cd
    )
    INSERT INTO merge_actions (
        table_name, action,
        cny_cd, asy_svc_typ_cd, tm_prd_typ_cd, ccy_cd, dtr_cri_eff_dt, dtr_cri_end_dt, dtr_cri_vlu_a, cus_csf_typ_cd
    )
    SELECT
        'tinfchg', merge_action,
        cny_cd, asy_svc_typ_cd, tm_prd_typ_cd, ccy_cd, dtr_cri_eff_dt, dtr_cri_end_dt, dtr_cri_vlu_a, cus_csf_typ_cd
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_informationalaccessorialcharge_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_informationalaccessorialcharge_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;