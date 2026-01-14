#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("tmincri_stg")]
[PrimaryKey(
    nameof(GeopoliticalExportCountryCode),
    nameof(ServiceFeatureTypeCode),
    nameof(PackageCharacteristicTypeCode),
    nameof(ServiceTypeCode),
    nameof(DeterminingCriteriaTypeCode),
    nameof(DeterminingCriteriaUnitTypeCode),
    nameof(CommodityClassificationCode),
    nameof(DeliveryZoneNumber),
    nameof(DeterminingCriteriaEffectiveDate),
    nameof(ApprovalStatusCode))]
public class MinimumCriteriaStaging
{
    [StringLength(4)]
    [Column("gpu_xpt_cny_cd", TypeName = "char")]
    [Required]
    public string GeopoliticalExportCountryCode { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("svc_fea_typ_cd", TypeName = "char")]
    [Required]
    public string ServiceFeatureTypeCode { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("pkg_cha_typ_cd", TypeName = "char")]
    [Required]
    public string PackageCharacteristicTypeCode { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("svm_typ_cd", TypeName = "char")]
    [Required]
    public string ServiceTypeCode { get; set; } = string.Empty;

    [Column("dtr_cri_typ_cd", TypeName = "smallint")]
    [Required]
    public short DeterminingCriteriaTypeCode { get; set; }

    [StringLength(3)]
    [Column("dtr_cri_unt_typ_cd", TypeName = "char")]
    [Required]
    public string DeterminingCriteriaUnitTypeCode { get; set; } = string.Empty;

    [StringLength(4)]
    [Column("cmy_cls_cd", TypeName = "char")]
    [Required]
    public string CommodityClassificationCode { get; set; } = string.Empty;

    [StringLength(6)]
    [Column("del_zn_nr", TypeName = "char")]
    [Required]
    public string DeliveryZoneNumber { get; set; } = string.Empty;

    [Column("dtr_cri_eff_dt", TypeName = "date")]
    [Required]
    public DateTime DeterminingCriteriaEffectiveDate { get; set; }

    [Column("dtr_cri_end_dt", TypeName = "date")]
    [Required]
    public DateTime DeterminingCriteriaEndDate { get; set; }

    [Column("dtr_cri_vlu_te", TypeName = "decimal(11,2)")]
    [Required]
    public decimal DeterminingCriteriaValue { get; set; }

    [StringLength(2)]
    [Column("apv_sts_cd", TypeName = "char")]
    [Required]
    public string ApprovalStatusCode { get; set; } = string.Empty;

    [Column("is_completed_ir", TypeName = "smallint")]
    [Required]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
