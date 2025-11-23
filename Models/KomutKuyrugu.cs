using System;
using System.ComponentModel.DataAnnotations; 

namespace ControlOverWeb.Models
{
    public class KomutKuyrugu
    {
        [Key]
        public long KomutID { get; set; } // Primary Key 
        public string KomutIcerigi { get; set; }
        public string Durum { get; set; } = "Bekliyor"; 
        public DateTime OlusturmaZamani { get; set; } = DateTime.UtcNow;
        public DateTime? IslemeAlmaZamani { get; set; } 
        public DateTime? TamamlanmaZamani { get; set; } 

        // -----------------------------------------------
      
        public int CihazID { get; set; }

        public Cihaz Cihaz { get; set; }
    }
}