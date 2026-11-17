using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StokTakip.Api.Dtos;
using StokTakip.Data;
using StokTakip.Models;

namespace StokTakip.Api.Controllers;

[ApiController]
[Route("api/depolar")]
[Authorize]
public class DepolarController : ControllerBase
{
    private readonly AppDbContext _db;

    public DepolarController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<DepoCevabi>>> Listele()
    {
        var depolar = await _db.Depolar
            .AsNoTracking()
            .OrderBy(d => d.Ad)
            .ToListAsync();

        var hareketler = await _db.StokHareketleri
            .AsNoTracking()
            .Include(h => h.Urun)
            .ToListAsync();

        var sonuc = depolar.Select(d =>
        {
            var depoUrunleri = hareketler
                .Where(h => h.DepoId == d.Id && h.Urun != null)
                .GroupBy(h => new { h.UrunId, h.Urun!.UrunKodu, h.Urun.Ad })
                .Select(g => new DepoUrunStokDto
                {
                    UrunId = g.Key.UrunId,
                    UrunKodu = g.Key.UrunKodu,
                    UrunAdi = g.Key.Ad,
                    Miktar = g.Sum(x => x.HareketTuru == "Cikis" ? -x.Miktar : x.Miktar)
                })
                .Where(x => x.Miktar > 0)
                .OrderBy(x => x.UrunAdi)
                .ToList();

            return new DepoCevabi
            {
                Id = d.Id,
                Ad = d.Ad,
                Konum = d.Konum,
                Aciklama = d.Aciklama,
                UrunCesidiSayisi = depoUrunleri.Count,
                ToplamUrunAdeti = depoUrunleri.Sum(x => x.Miktar),
                Urunler = depoUrunleri
            };
        }).ToList();

        return Ok(sonuc);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepoCevabi>> Getir(int id)
    {
        var depo = await _db.Depolar.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (depo == null)
            return NotFound(new { mesaj = "Depo bulunamadı." });

        var hareketler = await _db.StokHareketleri
            .AsNoTracking()
            .Include(h => h.Urun)
            .Where(h => h.DepoId == id && h.Urun != null)
            .ToListAsync();

        var urunler = hareketler
            .GroupBy(h => new { h.UrunId, h.Urun!.UrunKodu, h.Urun.Ad })
            .Select(g => new DepoUrunStokDto
            {
                UrunId = g.Key.UrunId,
                UrunKodu = g.Key.UrunKodu,
                UrunAdi = g.Key.Ad,
                Miktar = g.Sum(x => x.HareketTuru == "Cikis" ? -x.Miktar : x.Miktar)
            })
            .Where(x => x.Miktar > 0)
            .OrderBy(x => x.UrunAdi)
            .ToList();

        return Ok(new DepoCevabi
        {
            Id = depo.Id,
            Ad = depo.Ad,
            Konum = depo.Konum,
            Aciklama = depo.Aciklama,
            UrunCesidiSayisi = urunler.Count,
            ToplamUrunAdeti = urunler.Sum(x => x.Miktar),
            Urunler = urunler
        });
    }

    [HttpPost]
    [Authorize(Roles = "Yonetici")]
    public async Task<ActionResult<DepoCevabi>> Ekle(DepoKaydetIstegi istek)
    {
        var ad = istek.Ad.Trim();
        if (await _db.Depolar.AnyAsync(d => d.Ad.ToLower() == ad.ToLower()))
            return Conflict(new { mesaj = "Bu isimde bir depo zaten kayıtlı." });

        var depo = new Depo
        {
            Ad = ad,
            Konum = string.IsNullOrWhiteSpace(istek.Konum) ? null : istek.Konum.Trim(),
            Aciklama = string.IsNullOrWhiteSpace(istek.Aciklama) ? null : istek.Aciklama.Trim()
        };

        _db.Depolar.Add(depo);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Getir), new { id = depo.Id }, new DepoCevabi
        {
            Id = depo.Id,
            Ad = depo.Ad,
            Konum = depo.Konum,
            Aciklama = depo.Aciklama
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Yonetici")]
    public async Task<ActionResult<DepoCevabi>> Guncelle(int id, DepoKaydetIstegi istek)
    {
        var depo = await _db.Depolar.FindAsync(id);
        if (depo == null)
            return NotFound(new { mesaj = "Depo bulunamadı." });

        var ad = istek.Ad.Trim();
        if (await _db.Depolar.AnyAsync(d => d.Id != id && d.Ad.ToLower() == ad.ToLower()))
            return Conflict(new { mesaj = "Bu isimde başka bir depo zaten var." });

        depo.Ad = ad;
        depo.Konum = string.IsNullOrWhiteSpace(istek.Konum) ? null : istek.Konum.Trim();
        depo.Aciklama = string.IsNullOrWhiteSpace(istek.Aciklama) ? null : istek.Aciklama.Trim();
        await _db.SaveChangesAsync();

        return Ok(new DepoCevabi
        {
            Id = depo.Id,
            Ad = depo.Ad,
            Konum = depo.Konum,
            Aciklama = depo.Aciklama
        });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Yonetici")]
    public async Task<IActionResult> Sil(int id)
    {
        var depo = await _db.Depolar.FindAsync(id);
        if (depo == null)
            return NotFound(new { mesaj = "Depo bulunamadı." });

        if (await _db.StokHareketleri.AnyAsync(h => h.DepoId == id) ||
            await _db.StokIstekleri.AnyAsync(t => t.DepoId == id))
        {
            return BadRequest(new { mesaj = "Bu depoya ait stok hareketi bulunduğu için silinemez." });
        }

        _db.Depolar.Remove(depo);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

