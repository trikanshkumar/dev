using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Data.Models
{
    public class BaseEntity
    {
        [Required]
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        [Required]
        public long BatchId { get; set; }

        [StringLength(255)]
        [Required]
        public string DataLoadFile { get; set; }


    }
}
