using System;

namespace ControlOverWeb.Models
{
    public class Command
    {
        public int Id { get; set; }
        public string CommandCode { get; set; } = string.Empty; // Komut Kodu (W, A, S, D)
        public string Param { get; set; } = string.Empty; // Ekstra Parametreler
        
        public int TargetReceiverId { get; set; } // Hedef Robot ID
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsExecuted { get; set; } = false;
        public DateTime? ExecutedTime { get; set; }
    }
}
