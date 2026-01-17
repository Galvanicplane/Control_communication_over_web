using Microsoft.AspNetCore.Mvc;
using ControlOverWeb.Data;
using ControlOverWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlOverWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public StatsController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _context.SiteStats.FirstOrDefaultAsync();
            if (stats == null)
            {
                stats = new SiteStats { VisitorCount = 0, TotalCommandsSent = 0 };
                _context.SiteStats.Add(stats);
                await _context.SaveChangesAsync();
            }

            var onlineRobots = await _context.Receivers.CountAsync(r => r.IsOnline);

            return Ok(new { 
                stats.VisitorCount, 
                stats.TotalCommandsSent, 
                OnlineRobots = onlineRobots 
            });
        }
        
        [HttpPost("visit")]
        public async Task<IActionResult> RecordVisit()
        {
             var stats = await _context.SiteStats.FirstOrDefaultAsync();
             if (stats == null)
             {
                 stats = new SiteStats();
                 _context.SiteStats.Add(stats);
             }
             stats.VisitorCount++;
             await _context.SaveChangesAsync();
             return Ok();
        }
    }
}
