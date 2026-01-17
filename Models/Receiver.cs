using System;

namespace ControlOverWeb.Models
{
    public class Receiver
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SessionPassword { get; set; } = string.Empty; // Oturum Şifresi
        public bool IsOnline { get; set; }
        public DateTime LastHeartbeat { get; set; }
        public string Type { get; set; } = "Generic"; // Robot Tipi

        // Aktif Kontrolcü ID
        public int? ConnectedSenderId { get; set; }
    }
}
