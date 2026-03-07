CREATE OR REPLACE PROCEDURE sp_limitvaluesbasedoncriteria_merge_proc(
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
        cri_grp_cd char(2),
        cri_typ_cd char(2),
        apv_sts_cd char(2),
        ccy_cd char(3),
        dtr_cri_eff_dt date
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               cny_cd,
               cri_grp_cd,
               cri_typ_cd,
               apv_sts_cd,
               ccy_cd,
               dtr_cri_eff_dt,
               dtr_cri_end_dt,
               dcl_vlu_max_a,
               load_ref_te
          FROM tlmtvlu_stg
    ),
    cc_merge AS (
        MERGE INTO tlmtvlu AS tgt
        USING src_dedup AS src
        ON (
            tgt.cny_cd         = src.cny_cd AND
            tgt.cri_grp_cd     = src.cri_grp_cd AND
            tgt.cri_typ_cd     = src.cri_typ_cd AND
            tgt.apv_sts_cd     = src.apv_sts_cd AND
            tgt.ccy_cd         = src.ccy_cd AND
            tgt.dtr_cri_eff_dt = src.dtr_cri_eff_dt
        )
        WHEN MATCHED AND (
            tgt.dtr_cri_end_dt IS DISTINCT FROM src.dtr_cri_end_dt OR
            tgt.dcl_vlu_max_a  IS DISTINCT FROM src.dcl_vlu_max_a 
        ) THEN
            UPDATE SET
                dtr_cri_end_dt = src.dtr_cri_end_dt,
                dcl_vlu_max_a  = src.dcl_vlu_max_a,
                load_ref_te    = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                cny_cd,
                cri_grp_cd,
                cri_typ_cd,
                apv_sts_cd,
                ccy_cd,
                dtr_cri_eff_dt,
                dtr_cri_end_dt,
                dcl_vlu_max_a,
                load_ref_te
            ) VALUES (
                src.cny_cd,
                src.cri_grp_cd,
                src.cri_typ_cd,
                src.apv_sts_cd,
                src.ccy_cd,
                src.dtr_cri_eff_dt,
                src.dtr_cri_end_dt,
                src.dcl_vlu_max_a,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.cny_cd, src.cny_cd)                 AS cny_cd,
            COALESCE(tgt.cri_grp_cd, src.cri_grp_cd)         AS cri_grp_cd,
            COALESCE(tgt.cri_typ_cd, src.cri_typ_cd)         AS cri_typ_cd,
            COALESCE(tgt.apv_sts_cd, src.apv_sts_cd)         AS apv_sts_cd,
            COALESCE(tgt.ccy_cd, src.ccy_cd)                 AS ccy_cd,
            COALESCE(tgt.dtr_cri_eff_dt, src.dtr_cri_eff_dt) AS dtr_cri_eff_dt
    )
    INSERT INTO merge_actions (
        table_name,
        action,
        cny_cd,
        cri_grp_cd,
        cri_typ_cd,
        apv_sts_cd,
        ccy_cd,
        dtr_cri_eff_dt
    )
    SELECT
        'tlmtvlu',
        merge_action,
        cny_cd,
        cri_grp_cd,
        cri_typ_cd,
        apv_sts_cd,
        ccy_cd,
        dtr_cri_eff_dt
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_limitvaluesbasedoncriteria_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_limitvaluesbasedoncriteria_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;