using System;

namespace ControlOverWeb.Models
{
    public class Log
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Level { get; set; } = "Info"; // Log Seviyesi (Info/Warn)
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public int? RelatedReceiverId { get; set; } // İlgili Robot ID
    }
}
