#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TVSVCPK (Valid Origin Service Package) main table.
    /// </summary>
    [Table("tvsvcpk")]
    [PrimaryKey(
        nameof(ExportCountryCode),
        nameof(ServiceTypeCode),
        nameof(PackageCharacteristicTypeCode),
        nameof(ApprovalStatusCode),
        nameof(RecordEffectiveStartDate)
    )]
    public class ValidOriginServicePackage
    {
        [Required, StringLength(4)]
        [Column("gpn_xpt_cny_cd", TypeName = "char")]
        public string ExportCountryCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("pkg_cha_typ_cd", TypeName = "char")]
        public string PackageCharacteristicTypeCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("apv_sts_cd", TypeName = "char")]
        public string ApprovalStatusCode { get; set; } = string.Empty;

        [Required]
        [Column("rec_eff_stt_dt", TypeName = "date")]
        public DateTime RecordEffectiveStartDate { get; set; }

        [Required]
        [Column("rec_eff_end_dt", TypeName = "date")]
        public DateTime RecordEffectiveEndDate { get; set; }

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
