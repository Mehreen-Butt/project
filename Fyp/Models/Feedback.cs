namespace Fyp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Feedback")]
    public partial class Feedback
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [StringLength(200)]
        public string MESSAGE { get; set; }
        [Required]
        [StringLength(50)]
        public string EMAIL { get; set; }
        [Required]
        [StringLength(50)]
        public string NAME { get; set; }
        [Required]
        [StringLength(50)]
        public string CONTACT { get; set; }

        public int CUSTOMER_FID { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
