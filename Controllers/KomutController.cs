using Microsoft.AspNetCore.Mvc;             //api controller ve yanıtlar
using Microsoft.EntityFrameworkCore;        //EF core ve sql sorgu metodları 
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

        [HttpPost("gonder")]
        public async Task<IActionResult> KomutGonder([FromBody] KomutGondermeIstegi istek)
        {
            if (istek == null || istek.Komut == null )
            {
                return BadRequest("Komut bos"); 
            }

           
            var yeniKomut = new KomutKuyrugu
            {
                CihazID = istek.CihazID,
                KomutIcerigi = JsonSerializer.Serialize(istek.Komut)
            };

            await _context.KomutKuyrugu.AddAsync(yeniKomut);    //komut hazır
            await _context.SaveChangesAsync();                  //insert eder

            return Ok(new { KomutID = yeniKomut.KomutID, Durum = yeniKomut.Durum }); //200
        }


        [HttpGet("al/{cihazId}")]
        public async Task<IActionResult> KomutAl(int cihazId)
        {
            var siradakiKomut = await _context.KomutKuyrugu                 //sql sorgusu
                .Where(k => k.CihazID == cihazId && k.Durum == "Bekliyor") 
                .OrderBy(k => k.OlusturmaZamani)
                .FirstOrDefaultAsync(); 

            if (siradakiKomut == null)
            {
                return NoContent();
            }

            siradakiKomut.Durum = "IslemeAlindi";
            siradakiKomut.IslemeAlmaZamani = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(siradakiKomut);
        }

    }


}