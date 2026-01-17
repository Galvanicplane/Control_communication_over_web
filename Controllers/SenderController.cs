using Microsoft.AspNetCore.Mvc;
using ControlOverWeb.Data;
using ControlOverWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlOverWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SenderController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public SenderController(ApiDbContext context)
        {
            _context = context;
        }

        // Robot Listeleme
        [HttpGet("receivers")]
        public async Task<IActionResult> GetReceivers()
        {
            // Ölü Robot Temizliği (2dk)
            var cutoff = DateTime.UtcNow.AddMinutes(-2);
            var deadRobots = await _context.Receivers.Where(r => r.LastHeartbeat < cutoff).ToListAsync();
            if(deadRobots.Any()) {
                _context.Receivers.RemoveRange(deadRobots);
                await _context.SaveChangesAsync();
            }

            // Aktif Robot Filtresi
            var robots = await _context.Receivers
                .Where(r => r.IsOnline) 
                .Select(r => new { r.Id, r.Name, r.Type, IsBusy = r.ConnectedSenderId != null })
                .ToListAsync();
            return Ok(robots);
        }

        // Bağlantı İsteği
        [HttpPost("connect")]
        public async Task<IActionResult> Connect([FromBody] ConnectRequest request)
        {
            var receiver = await _context.Receivers.FindAsync(request.ReceiverId);
            if (receiver == null) return NotFound("Robot not found");

            if (receiver.SessionPassword != request.Password)
            {
                return Unauthorized("Yanlış robot şifresi!");
            }

            // Oturum Kaydı (Sender)
            var sender = new Sender
            {
                Name = request.SenderName,
                LastActiveTime = DateTime.UtcNow
            };
            _context.Senders.Add(sender);
            await _context.SaveChangesAsync();

            // Eşleştirme
            receiver.ConnectedSenderId = sender.Id;
            
            // Geçmiş Kaydı Logu
            _context.ConnectionHistory.Add(new ConnectionHistory {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                StartTime = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new { SenderId = sender.Id, Message = "Connected!" });
        }

        // Komut Gönderimi
        [HttpPost("command")]
        public async Task<IActionResult> SendCommand([FromBody] CommandRequest request)
        {
            var sender = await _context.Senders.FindAsync(request.SenderId);
            if (sender == null) return Unauthorized();

            sender.LastActiveTime = DateTime.UtcNow;

            var cmd = new Command
            {
                CommandCode = request.Code,
                Param = request.Param,
                TargetReceiverId = request.ReceiverId,
                CreatedAt = DateTime.UtcNow,
                IsExecuted = false
            };

            _context.Commands.Add(cmd);

            // İstatistik Güncelleme
            var stats = await _context.SiteStats.FirstOrDefaultAsync();
            if (stats == null) {
                 stats = new SiteStats();
                 _context.SiteStats.Add(stats);
            }
            stats.TotalCommandsSent++;

            await _context.SaveChangesAsync();
            return Ok();
        }

        // Bağlantı Kesme (Disconnect)
        [HttpPost("disconnect")]
        public async Task<IActionResult> Disconnect([FromBody] DisconnectRequest request)
        {
            var receiver = await _context.Receivers.FindAsync(request.ReceiverId);
            if (receiver != null && receiver.ConnectedSenderId == request.SenderId)
            {
                receiver.ConnectedSenderId = null;
                
                // Oturum Kapanışı
                var history = await _context.ConnectionHistory
                    .OrderByDescending(h => h.StartTime)
                    .FirstOrDefaultAsync(h => h.SenderId == request.SenderId && h.ReceiverId == request.ReceiverId);
                
                if(history != null) history.EndTime = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }

    public class ConnectRequest
    {
        public int ReceiverId { get; set; }
        public string SenderName { get; set; }
        public string Password { get; set; }
    }

    public class CommandRequest
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Code { get; set; }
        public string Param { get; set; }
    }

    public class DisconnectRequest
    {
        public int ReceiverId { get; set; }
        public int SenderId { get; set; }
    }
}
