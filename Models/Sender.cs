using System;

namespace ControlOverWeb.Models
{
    public class Sender
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Operatör Adı
        public DateTime LastActiveTime { get; set; }
    }
}
