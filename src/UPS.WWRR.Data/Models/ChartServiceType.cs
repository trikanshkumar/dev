#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the ZCHARTSVCTYP (Chart Service Type) table.
    /// </summary>
    [Table("zchartsvctyp")]
    [PrimaryKey(nameof(CountryCode), nameof(GeopoliticalNumber), nameof(ServiceTypeCode))]
    public class ChartServiceType
    {
        [Required, StringLength(2)]
        [Column("cny_cd", TypeName = "char")]
        public string CountryCode { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("gpu_nr", TypeName = "char")]
        public string GeopoliticalNumber { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
