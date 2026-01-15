using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlOverWeb.Data;
using ControlOverWeb.Models;
using System.Text.Json;

namespace ControlOverWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KomutController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public KomutController(ApiDbContext context)
        {
            _context = context;
        }

        // Komut Gonderme Islemi
        [HttpPost("gonder")]
        public async Task<IActionResult> KomutGonder([FromBody] KomutGondermeIstegi istek)
        {
            if (istek == null || istek.Komut == null)
            {
                return BadRequest("Komut bos");
            }

            // Yeni komutu hazirla
            var yeniKomut = new KomutKuyrugu
            {
                CihazID = istek.CihazID,
                KomutIcerigi = JsonSerializer.Serialize(istek.Komut)
            };

            // Veritabanina ekle
            await _context.KomutKuyrugu.AddAsync(yeniKomut);
            await _context.SaveChangesAsync();

            return Ok(new { KomutID = yeniKomut.KomutID, Durum = yeniKomut.Durum });
        }

        // Cihazin Bekleyen Komutunu Cekmesi
        [HttpGet("al/{cihazId}")]
        public async Task<IActionResult> KomutAl(int cihazId)
        {
            // Bekleyen en eski komutu bul
            var siradakiKomut = await _context.KomutKuyrugu
                .Where(k => k.CihazID == cihazId && k.Durum == "Bekliyor")
                .OrderBy(k => k.OlusturmaZamani)
                .FirstOrDefaultAsync();

            if (siradakiKomut == null)
            {
                return NoContent();
            }

            // Durumu guncelle
            siradakiKomut.Durum = "IslemeAlindi";
            siradakiKomut.IslemeAlmaZamani = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(siradakiKomut);
        }

        [HttpDelete("sil/{komutId}")]
        public async Task<IActionResult> KomutSil(int komutId)
        {
            var silinecekKomut = await _context.KomutKuyrugu.FindAsync(komutId);

            if (silinecekKomut == null)
            {
                return NotFound($"ID'si {komutId} olan komut bulunamadi.");
            }

            _context.KomutKuyrugu.Remove(silinecekKomut);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Komut ID: {komutId} silindi." });
        }

        [HttpGet("cihazlar")]
        public async Task<IActionResult> CihazlariListele()
        {
            var cihazListesi = await _context.Cihazlar.ToListAsync();

            if (cihazListesi == null || cihazListesi.Count == 0)
            {
                return NoContent();
            }

            return Ok(cihazListesi);
        }
    }
}