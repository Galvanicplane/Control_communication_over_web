using System.Collections.Generic; 

namespace ControlOverWeb.Models
{
    public class Cihaz
    {
        public int CihazID { get; set; } // Primary Key
        public string CihazAdi { get; set; }
        public string CihazToken { get; set; }
        public DateTime? SonGorulme { get; set; }

      //----------------------------------------------------
        public ICollection<KomutKuyrugu> Komutlar { get; set; }
    }
}