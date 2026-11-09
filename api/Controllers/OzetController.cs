using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;

namespace StokTakip.Api.Controllers;

[ApiController]
[Route("api/ozet")]
[Authorize]
public class OzetController : ControllerBase
{
    private readonly AppDbContext _db;

    public OzetController(AppDbContext db) => _db = db;

    /// <summary>Ana sayfa istatistikleri. Kritik stok: mevcut miktar, kritik seviyeye eşit veya altında.</summary>
    [HttpGet]
    public async Task<IActionResult> Getir()
    {
        var urunCesidi = await _db.Urunler.CountAsync();
        var toplamStok = await _db.Urunler.SumAsync(u => (long?)u.MevcutMiktar) ?? 0;
        var kritikSayisi = await _db.Urunler
            .CountAsync(u => u.MevcutMiktar <= u.KritikStokSeviyesi);

        return Ok(new
        {
            urunCesidi,
            toplamStok,
            kritikStokUrunSayisi = kritikSayisi
        });
    }
}

