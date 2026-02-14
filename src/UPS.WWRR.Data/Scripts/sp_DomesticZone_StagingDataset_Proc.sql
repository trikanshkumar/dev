CREATE OR REPLACE PROCEDURE sp_domesticzone_stagingdataset_proc(
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
DECLARE
	v_domzchartsts_ins integer;
	v_domzchartsts_upd integer;
	v_domzchartsts_del integer;
	v_domzchartlkup_ins integer;
	v_tdozndt_ins integer;
	v_tdoznhd_ins integer;
	v_domzchartdtngeo_ins integer;
	v_domzchartorggeo_ins integer;
BEGIN
    -- Clear normalized staging tables before populating
    TRUNCATE TABLE domzchartsts_stg;
    TRUNCATE TABLE domzchartlkup_stg;
    TRUNCATE TABLE tdoznhd_new_stg;
	TRUNCATE TABLE tdozndt_new_stg;
    TRUNCATE TABLE domzchartdtngeo_stg;
    TRUNCATE TABLE domzchartorggeo_stg;

    -- 1. DOMZCHARTSTS (Domestic Zone Chart Status) - Insert into staging first

INSERT INTO domzchartsts_stg(
    zch_nr ,
    dom_zn_hdr_stt_dt ,
    dom_zn_hdr_end_dt ,
    bus_eny_acs_sts_cd 
)
select distinct
zch_nr,
dom_zn_hdr_stt_dt ,
dom_zn_hdr_end_dt ,
bus_eny_acs_sts_cd 
from tdoznhd_stg
order by zch_nr;

    -- Create temp table to track merge actions for DOMZCHARTSTS
    CREATE TEMP TABLE IF NOT EXISTS domzchartsts_merge_actions (
        action text
    );
    TRUNCATE TABLE domzchartsts_merge_actions;

    -- MERGE domzchartsts_stg to actual domzchartsts table
    WITH src_dedup AS (
        SELECT DISTINCT
               zch_nr,
               dom_zn_hdr_stt_dt,
               dom_zn_hdr_end_dt,
               bus_eny_acs_sts_cd,
               load_ref_te
          FROM domzchartsts_stg
    ),
    sts_merge AS (
        MERGE INTO domzchartsts AS tgt
        USING src_dedup AS src
        ON (
            tgt.zch_nr              = src.zch_nr AND
            tgt.dom_zn_hdr_stt_dt   = src.dom_zn_hdr_stt_dt AND
            tgt.dom_zn_hdr_end_dt   = src.dom_zn_hdr_end_dt 
        )
        WHEN MATCHED AND (
            tgt.bus_eny_acs_sts_cd IS DISTINCT FROM src.bus_eny_acs_sts_cd OR
            tgt.load_ref_te IS DISTINCT FROM src.load_ref_te
        ) THEN UPDATE SET
            bus_eny_acs_sts_cd = src.bus_eny_acs_sts_cd,
            load_ref_te = src.load_ref_te
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                zch_nr,
                dom_zn_hdr_stt_dt,
                dom_zn_hdr_end_dt,
                bus_eny_acs_sts_cd,
                load_ref_te
            ) VALUES (
                src.zch_nr,
                src.dom_zn_hdr_stt_dt,
                src.dom_zn_hdr_end_dt,
                src.bus_eny_acs_sts_cd,
                src.load_ref_te
            )
        WHEN NOT MATCHED BY SOURCE THEN
            DELETE
        RETURNING merge_action()
    )
    INSERT INTO domzchartsts_merge_actions (action)
    SELECT merge_action FROM sts_merge;

    -- Get counts for DOMZCHARTSTS merge
    SELECT
        COUNT(*) FILTER (WHERE action = 'INSERT'),
        COUNT(*) FILTER (WHERE action = 'UPDATE'),
        COUNT(*) FILTER (WHERE action = 'DELETE')
    INTO v_domzchartsts_ins, v_domzchartsts_upd, v_domzchartsts_del
    FROM domzchartsts_merge_actions;

    DROP TABLE IF EXISTS domzchartsts_merge_actions;

 -- 2. DOMZCHARTLKUP (Domestic Zone Chart Lookup)

INSERT INTO domzchartlkup_stg(
    zch_nr ,
    zch_sht_dsc_te ,
    zch_lg_dsc_te 
)
select distinct
zch_nr,
zch_sht_dsc_te,
zch_lg_dsc_te
from tdoznhd_stg;

 GET DIAGNOSTICS v_domzchartlkup_ins = ROW_COUNT;

  -- 3. DOMZCHARTORGGEO (Domestic Zone Chart Origin Geo)
    -- Aggregate by chart-status/origin key to avoid duplicates
    -- Use zch_sts_nr from actual domzchartsts table (not staging) to get the correct identity values


INSERT INTO domzchartorggeo_stg(
    zch_sts_nr,
    org_cny_cd ,
    org_gpu_nr ,
    org_rng_lo_psl_cd ,  
    org_rng_hi_psl_cd 
)
select distinct 
zcs.zch_sts_nr,
t.org_cny_cd ,
t.org_gpu_nr ,
t.org_rng_lo_psl_cd ,  
t.org_rng_hi_psl_cd 
from tdozndt_stg t
join domzchartsts zcs
	on zcs.zch_nr = t.zch_nr
	and zcs.dom_zn_hdr_stt_dt = t.dom_zn_hdr_stt_dt
	and zcs.dom_zn_hdr_end_dt = t.dom_zn_dtl_end_dt 
	and zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd;

    GET DIAGNOSTICS v_domzchartorggeo_ins = ROW_COUNT;

    -- 4. DOMZCHARTDTNGEO (Domestic Zone Chart Destination Geo)
    -- Aggregate by chart-status/destination key to avoid duplicate PK violations
    -- Use zch_sts_nr from actual domzchartsts table (not staging) to get the correct identity values

INSERT INTO domzchartdtngeo_stg(
	zch_sts_nr, 
    dtn_cny_cd ,
    dtn_gpu_nr ,
    dtn_rng_lo_psl_cd ,  
    dtn_rng_hi_psl_cd ,
    del_zn_nr 
)
select distinct 
zcs.zch_sts_nr,
dtn_cny_cd ,
dtn_gpu_nr ,
dtn_rng_lo_psl_cd ,  
dtn_rng_hi_psl_cd ,
del_zn_nr
from tdozndt_stg t
join domzchartsts zcs
	on zcs.zch_nr = t.zch_nr
	and zcs.dom_zn_hdr_stt_dt = t.dom_zn_hdr_stt_dt
	and zcs.dom_zn_hdr_end_dt = t.dom_zn_dtl_end_dt 
	and zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd;

    GET DIAGNOSTICS v_domzchartdtngeo_ins = ROW_COUNT;


-- 5. TDOZNDT (Domestic Zone Detail New) - from tdozndt_stg with join to actual domzchartsts
    -- We aggregate by zch_sts_nr to avoid duplicate key violations on tdozndt_new_stg
    -- Use zch_sts_nr from actual domzchartsts table (not staging) to get the correct identity values

INSERT INTO tdozndt_new_stg(
    zch_sts_nr, 
    svc_typ_cd ,
    zn_ncv_typ_cd 
)
SELECT DISTINCT 
	zcs.zch_sts_nr,
    svc_typ_cd,
    zn_ncv_typ_cd
FROM tdozndt_stg t
join domzchartsts zcs
	on zcs.zch_nr = t.zch_nr
	and zcs.dom_zn_hdr_stt_dt = t.dom_zn_hdr_stt_dt
	and zcs.dom_zn_hdr_end_dt = t.dom_zn_dtl_end_dt 
	and zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd;

    GET DIAGNOSTICS v_tdozndt_ins = ROW_COUNT;

-- 6. TDOZNHD (Domestic Zone Header New) - normalized
    -- We aggregate by zch_sts_nr to avoid duplicate key violations on tdoznhd_new_stg
    -- Use zch_sts_nr from actual domzchartsts table (not staging) to get the correct identity values

INSERT INTO tdoznhd_new_stg(
    zch_sts_nr,
    svc_typ_cd , 
    mvm_drc_cd 
)
select distinct
zcs.zch_sts_nr,
t.svc_typ_cd, 
t.mvm_drc_cd
from tdoznhd_stg t
join domzchartsts zcs
	on zcs.zch_nr = t.zch_nr
	and zcs.dom_zn_hdr_stt_dt = t.dom_zn_hdr_stt_dt
	and zcs.dom_zn_hdr_end_dt = t.dom_zn_hdr_end_dt 
	and zcs.bus_eny_acs_sts_cd = t.bus_eny_acs_sts_cd;

    GET DIAGNOSTICS v_tdoznhd_ins = ROW_COUNT;


	-- aggregate inserts across all normalized staging tables
	-- DOMZCHARTSTS counts come from merge, others are inserts into staging tables
	insertcount := coalesce(v_domzchartsts_ins, 0)
		+ coalesce(v_domzchartlkup_ins, 0)
		+ coalesce(v_tdozndt_ins, 0)
		+ coalesce(v_tdoznhd_ins, 0)
		+ coalesce(v_domzchartdtngeo_ins, 0)
		+ coalesce(v_domzchartorggeo_ins, 0);
	updatecount := coalesce(v_domzchartsts_upd, 0);
	deletecount := coalesce(v_domzchartsts_del, 0);

    errornumber    := NULL;
    errorstate     := NULL;
    errorprocedure := 'sp_domesticzone_stagingdataset_proc';
    errorline      := NULL;
    errormessage   := NULL;

EXCEPTION WHEN OTHERS THEN
    errornumber    := SQLSTATE;
    errorstate     := SQLSTATE;
    errorprocedure := 'sp_domesticzone_stagingdataset_proc';
    GET STACKED DIAGNOSTICS errorline = PG_EXCEPTION_CONTEXT;
    errormessage   := SQLERRM;
	insertcount := 0;
	updatecount := 0;
	deletecount := 0;
END;
$BODY$;
