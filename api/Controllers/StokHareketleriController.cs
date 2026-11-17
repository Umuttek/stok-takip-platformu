using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StokTakip.Api.Dtos;
using StokTakip.Data;
using StokTakip.Models;

namespace StokTakip.Api.Controllers;

[ApiController]
[Route("api/stok-hareketleri")]
[Authorize]
public class StokHareketleriController : ControllerBase
{
    private readonly AppDbContext _db;

    public StokHareketleriController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<StokHareketiCevabi>>> Listele(
        [FromQuery] int? urunId,
        [FromQuery] int? depoId,
        [FromQuery] int adet = 50)
    {
        var sorgu = _db.StokHareketleri
            .AsNoTracking()
            .Include(h => h.Urun)
            .Include(h => h.Depo)
            .Include(h => h.Kullanici)
            .AsQueryable();

        if (urunId.HasValue)
            sorgu = sorgu.Where(h => h.UrunId == urunId.Value);
        if (depoId.HasValue)
            sorgu = sorgu.Where(h => h.DepoId == depoId.Value);

        var liste = await sorgu
            .OrderByDescending(h => h.Tarih)
            .ThenByDescending(h => h.Id)
            .Take(Math.Clamp(adet, 1, 200))
            .Select(h => new StokHareketiCevabi
            {
                Id = h.Id,
                UrunId = h.UrunId,
                UrunKodu = h.Urun != null ? h.Urun.UrunKodu : "",
                UrunAdi = h.Urun != null ? h.Urun.Ad : "",
                DepoId = h.DepoId,
                DepoAdi = h.Depo != null ? h.Depo.Ad : "",
                HareketTuru = h.HareketTuru,
                Miktar = h.Miktar,
                Tarih = h.Tarih,
                KullaniciAdSoyad = h.Kullanici != null
                    ? (!string.IsNullOrWhiteSpace(h.Kullanici.AdSoyad) ? h.Kullanici.AdSoyad : (h.Kullanici.Email ?? ""))
                    : ""
            })
            .ToListAsync();

        return Ok(liste);
    }

    [HttpPost]
    public async Task<ActionResult<StokHareketiCevabi>> Ekle(StokHareketiEkleIstegi istek)
    {
        var urun = await _db.Urunler.FindAsync(istek.UrunId);
        if (urun == null)
            return NotFound(new { mesaj = "Seçilen ürün bulunamadı." });

        var depo = await _db.Depolar.FindAsync(istek.DepoId);
        if (depo == null)
            return NotFound(new { mesaj = "Seçilen depo bulunamadı." });

        var depoHareketleri = await _db.StokHareketleri
            .AsNoTracking()
            .Where(h => h.UrunId == istek.UrunId && h.DepoId == istek.DepoId)
            .ToListAsync();

        var depodakiMiktar = depoHareketleri
            .Sum(h => h.HareketTuru == "Cikis" ? -h.Miktar : h.Miktar);

        if (istek.HareketTuru == "Cikis")
        {
            if (depodakiMiktar < istek.Miktar)
            {
                return BadRequest(new
                {
                    mesaj = $"\"{depo.Ad}\" deposunda yeterli \"{urun.Ad}\" stoğu yok. Bu depodaki mevcut miktar: {depodakiMiktar}."
                });
            }
            urun.MevcutMiktar = Math.Max(0, urun.MevcutMiktar - istek.Miktar);
        }
        else
        {
            urun.MevcutMiktar += istek.Miktar;
        }

        var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var kullaniciAdi = User.FindFirstValue(ClaimTypes.Name) ?? "";

        var hareket = new StokHareketi
        {
            UrunId = urun.Id,
            DepoId = depo.Id,
            HareketTuru = istek.HareketTuru,
            Miktar = istek.Miktar,
            Tarih = DateTime.UtcNow,
            KullaniciId = kullaniciId
        };

        _db.StokHareketleri.Add(hareket);
        await _db.SaveChangesAsync();

        return Created(string.Empty, new StokHareketiCevabi
        {
            Id = hareket.Id,
            UrunId = urun.Id,
            UrunKodu = urun.UrunKodu,
            UrunAdi = urun.Ad,
            DepoId = depo.Id,
            DepoAdi = depo.Ad,
            HareketTuru = hareket.HareketTuru,
            Miktar = hareket.Miktar,
            Tarih = hareket.Tarih,
            KullaniciAdSoyad = kullaniciAdi
        });
    }
}

