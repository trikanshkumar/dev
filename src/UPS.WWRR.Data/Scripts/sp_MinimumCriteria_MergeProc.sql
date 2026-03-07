CREATE OR REPLACE PROCEDURE sp_minimumcriteria_merge_proc(
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
        gpu_xpt_cny_cd char(4),
        svc_fea_typ_cd char(3),
        pkg_cha_typ_cd char(3),
        svm_typ_cd char(3),
        dtr_cri_typ_cd smallint,
        dtr_cri_unt_typ_cd char(3),
        cmy_cls_cd char(4),
        del_zn_nr char(6),
        dtr_cri_eff_dt date,
        apv_sts_cd char(2)
    );

    WITH src_dedup AS (
        SELECT DISTINCT
               gpu_xpt_cny_cd,
               svc_fea_typ_cd,
               pkg_cha_typ_cd,
               svm_typ_cd,
               dtr_cri_typ_cd,
               dtr_cri_unt_typ_cd,
               cmy_cls_cd,
               del_zn_nr,
               dtr_cri_eff_dt,
               dtr_cri_end_dt,
               dtr_cri_vlu_te,
               apv_sts_cd,
               load_ref_te
          FROM tmincri_stg
    ),
    cc_merge AS (
        MERGE INTO tmincri AS tgt
        USING src_dedup AS src
        ON (
            tgt.gpu_xpt_cny_cd     = src.gpu_xpt_cny_cd AND
            tgt.svc_fea_typ_cd     = src.svc_fea_typ_cd AND
            tgt.pkg_cha_typ_cd     = src.pkg_cha_typ_cd AND
            tgt.svm_typ_cd         = src.svm_typ_cd AND
            tgt.dtr_cri_typ_cd     = src.dtr_cri_typ_cd AND
            tgt.dtr_cri_unt_typ_cd = src.dtr_cri_unt_typ_cd AND
            tgt.cmy_cls_cd         = src.cmy_cls_cd AND
            tgt.del_zn_nr          = src.del_zn_nr AND
            tgt.dtr_cri_eff_dt     = src.dtr_cri_eff_dt AND
            tgt.apv_sts_cd         = src.apv_sts_cd
        )
        WHEN MATCHED AND (
            tgt.dtr_cri_end_dt  IS DISTINCT FROM src.dtr_cri_end_dt OR
            tgt.dtr_cri_vlu_te  IS DISTINCT FROM src.dtr_cri_vlu_te 
        ) THEN
            UPDATE SET
                dtr_cri_end_dt = src.dtr_cri_end_dt,
                dtr_cri_vlu_te = src.dtr_cri_vlu_te,
                load_ref_te    = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                gpu_xpt_cny_cd,
                svc_fea_typ_cd,
                pkg_cha_typ_cd,
                svm_typ_cd,
                dtr_cri_typ_cd,
                dtr_cri_unt_typ_cd,
                cmy_cls_cd,
                del_zn_nr,
                dtr_cri_eff_dt,
                dtr_cri_end_dt,
                dtr_cri_vlu_te,
                apv_sts_cd,
                load_ref_te
            ) VALUES (
                src.gpu_xpt_cny_cd,
                src.svc_fea_typ_cd,
                src.pkg_cha_typ_cd,
                src.svm_typ_cd,
                src.dtr_cri_typ_cd,
                src.dtr_cri_unt_typ_cd,
                src.cmy_cls_cd,
                src.del_zn_nr,
                src.dtr_cri_eff_dt,
                src.dtr_cri_end_dt,
                src.dtr_cri_vlu_te,
                src.apv_sts_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING
            merge_action(),
            COALESCE(tgt.gpu_xpt_cny_cd,     src.gpu_xpt_cny_cd)     AS gpu_xpt_cny_cd,
            COALESCE(tgt.svc_fea_typ_cd,     src.svc_fea_typ_cd)     AS svc_fea_typ_cd,
            COALESCE(tgt.pkg_cha_typ_cd,     src.pkg_cha_typ_cd)     AS pkg_cha_typ_cd,
            COALESCE(tgt.svm_typ_cd,         src.svm_typ_cd)         AS svm_typ_cd,
            COALESCE(tgt.dtr_cri_typ_cd,     src.dtr_cri_typ_cd)     AS dtr_cri_typ_cd,
            COALESCE(tgt.dtr_cri_unt_typ_cd, src.dtr_cri_unt_typ_cd) AS dtr_cri_unt_typ_cd,
            COALESCE(tgt.cmy_cls_cd,         src.cmy_cls_cd)         AS cmy_cls_cd,
            COALESCE(tgt.del_zn_nr,          src.del_zn_nr)          AS del_zn_nr,
            COALESCE(tgt.dtr_cri_eff_dt,     src.dtr_cri_eff_dt)     AS dtr_cri_eff_dt,
            COALESCE(tgt.apv_sts_cd,         src.apv_sts_cd)         AS apv_sts_cd
    )
    INSERT INTO merge_actions (
        table_name,
        action,
        gpu_xpt_cny_cd,
        svc_fea_typ_cd,
        pkg_cha_typ_cd,
        svm_typ_cd,
        dtr_cri_typ_cd,
        dtr_cri_unt_typ_cd,
        cmy_cls_cd,
        del_zn_nr,
        dtr_cri_eff_dt,
        apv_sts_cd
    )
    SELECT
        'tmincri',
        merge_action,
        gpu_xpt_cny_cd,
        svc_fea_typ_cd,
        pkg_cha_typ_cd,
        svm_typ_cd,
        dtr_cri_typ_cd,
        dtr_cri_unt_typ_cd,
        cmy_cls_cd,
        del_zn_nr,
        dtr_cri_eff_dt,
        apv_sts_cd
    FROM cc_merge;

    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO InsertCount, UpdateCount, DeleteCount
    FROM merge_actions;

    ErrorNumber    := NULL;
    ErrorState     := NULL;
    ErrorProcedure := 'sp_minimumcriteria_merge_proc';
    ErrorLine      := NULL;
    ErrorMessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    ErrorNumber    := SQLSTATE;
    ErrorState     := SQLSTATE;
    ErrorProcedure := 'sp_minimumcriteria_merge_proc';
    GET STACKED DIAGNOSTICS ErrorLine = PG_EXCEPTION_CONTEXT;
    ErrorMessage   := SQLERRM;
    InsertCount := 0;
    UpdateCount := 0;
    DeleteCount := 0;
END;
$BODY$;
