using Microsoft.AspNetCore.Mvc;
using ControlOverWeb.Data;
using ControlOverWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace ControlOverWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RobotController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public RobotController(ApiDbContext context)
        {
            _context = context;
        }

        // Robot Kaydı (Register)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var receiver = await _context.Receivers.FirstOrDefaultAsync(r => r.Name == request.Name);
            bool isSpy = false;

            if (receiver == null)
            {
                receiver = new Receiver
                {
                    Name = request.Name,
                    Type = request.Type ?? "Generic",
                    IsOnline = true,
                    LastHeartbeat = DateTime.UtcNow,
                    SessionPassword = request.Password // Oturum Şifresi
                };
                _context.Receivers.Add(receiver);
            }
            else
            {
                if (receiver.IsOnline && receiver.LastHeartbeat > DateTime.UtcNow.AddSeconds(-15))
                {
                    // Şifre kontrolü
                    if (receiver.SessionPassword == request.Password)
                    {
                        isSpy = true;
                    }
                    
                }

                receiver.IsOnline = true;
                receiver.LastHeartbeat = DateTime.UtcNow;
                
                
            }
            
            await _context.Logs.AddAsync(new Log { 
                Message = isSpy ? $"Robot '{request.Name}' için Gözlemci (Spy) bağlandı." : $"Robot '{request.Name}' online oldu.", 
                RelatedReceiverId = receiver.Id 
            });

            await _context.SaveChangesAsync();
            return Ok(new { receiver.Id, Message = "Robot registered successfully", IsSpyMode = isSpy });
        }

        // Komut Sorgulama 
        [HttpGet("poll/{id}")]
        public async Task<IActionResult> PollCommands(int id, [FromQuery] bool peek = false)
        {
            // Heartbeat Güncelleme
            var receiver = await _context.Receivers.FindAsync(id);
            if (receiver != null)
            {
                receiver.LastHeartbeat = DateTime.UtcNow;
                receiver.IsOnline = true;
            }

            // Bekleyen Komutlar
            var commands = await _context.Commands
                .Where(c => c.TargetReceiverId == id && !c.IsExecuted)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            if (!commands.Any())
            {
                await _context.SaveChangesAsync(); // Heartbeat Kaydı
                return Ok(new List<object>()); // Boş Liste Dön
            }

            // Spy Mode
            if (!peek)
            {
                // Komut İşlendi İşareti
                foreach (var cmd in commands)
                {
                    cmd.IsExecuted = true;
                    cmd.ExecutedTime = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
            else 
            {
                await _context.SaveChangesAsync();
            }
            
            return Ok(commands.Select(c => new { c.CommandCode, c.Param }));
        }
    }

    public class RegisterRequest
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }
    }
}
