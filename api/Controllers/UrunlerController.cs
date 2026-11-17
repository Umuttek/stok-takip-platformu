using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StokTakip.Api.Dtos;
using StokTakip.Data;
using StokTakip.Models;

namespace StokTakip.Api.Controllers;

[ApiController]
[Route("api/urunler")]
[Authorize]
public class UrunlerController : ControllerBase
{
    private const long BarkodBaslangicDegeri = 8690000000000L;
    private readonly AppDbContext _db;
    private readonly string _gorselKlasoru;

    public UrunlerController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _gorselKlasoru = Path.Combine(env.ContentRootPath, "UrunGorselleri");
    }

    [HttpGet("sonraki-kod")]
    public async Task<ActionResult<object>> SonrakiKod()
    {
        var kod = await SonrakiUrunKodunuUretAsync();
        return Ok(new { urunKodu = kod });
    }

    [HttpGet]
    public async Task<ActionResult<List<UrunCevabi>>> Listele([FromQuery] string? arama)
    {
        var sorgu = _db.Urunler
            .AsNoTracking()
            .Include(u => u.Kategori)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var desen = $"%{arama.Trim()}%";
            sorgu = sorgu.Where(u =>
                EF.Functions.ILike(u.Ad, desen) ||
                EF.Functions.ILike(u.UrunKodu, desen));
        }

        var urunler = await sorgu.OrderBy(u => u.Ad).ToListAsync();
        var urunIdleri = urunler.Select(u => u.Id).ToList();

        var tumDepolar = await _db.Depolar
            .AsNoTracking()
            .OrderBy(d => d.Ad)
            .ToListAsync();

        var hareketler = await _db.StokHareketleri
            .AsNoTracking()
            .Where(h => urunIdleri.Contains(h.UrunId))
            .ToListAsync();

        var cevap = urunler
            .Select(u => UrunCevabiOlustur(u, tumDepolar, hareketler))
            .ToList();
        return Ok(cevap);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UrunCevabi>> Getir(int id)
    {
        var urun = await _db.Urunler
            .AsNoTracking()
            .Include(u => u.Kategori)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (urun == null)
            return NotFound(new { mesaj = "Ürün bulunamadı." });

        var tumDepolar = await _db.Depolar
            .AsNoTracking()
            .OrderBy(d => d.Ad)
            .ToListAsync();

        var hareketler = await _db.StokHareketleri
            .AsNoTracking()
            .Where(h => h.UrunId == id)
            .ToListAsync();

        return Ok(UrunCevabiOlustur(urun, tumDepolar, hareketler));
    }

    [HttpPost]
    [Authorize(Roles = "Yonetici")]
    public async Task<ActionResult<UrunCevabi>> Ekle(UrunEkleIstegi istek)
    {
        var ad = istek.Ad.Trim();

        var kategori = await _db.Kategoriler.FindAsync(istek.KategoriId);
        if (kategori == null)
            return BadRequest(new { mesaj = "Seçilen kategori bulunamadı." });

        var depo = await _db.Depolar.FindAsync(istek.DepoId);
        if (depo == null)
            return BadRequest(new { mesaj = "Seçilen depo bulunamadı." });

        var otomatikKod = await SonrakiUrunKodunuUretAsync();

        var urun = new Urun
        {
            UrunKodu = otomatikKod,
            Ad = ad,
            Aciklama = string.IsNullOrWhiteSpace(istek.Aciklama) ? null : istek.Aciklama.Trim(),
            KategoriId = istek.KategoriId,
            KritikStokSeviyesi = istek.KritikStokSeviyesi,
            MevcutMiktar = istek.MevcutMiktar
        };

        _db.Urunler.Add(urun);
        await _db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(istek.FotografBase64))
        {
            await FotografKaydetAsync(urun.Id, istek.FotografBase64);
        }

        // Eklenen ürün stoğunu seçilen depoya zimmetle
        var hareket = new StokHareketi
        {
            UrunId = urun.Id,
            DepoId = depo.Id,
            HareketTuru = "Giris",
            Miktar = istek.MevcutMiktar,
            Tarih = DateTime.UtcNow,
            KullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };
        _db.StokHareketleri.Add(hareket);
        await _db.SaveChangesAsync();

        var tumDepolar = await _db.Depolar.AsNoTracking().OrderBy(d => d.Ad).ToListAsync();
        urun.Kategori = kategori;

        return CreatedAtAction(
            nameof(Getir),
            new { id = urun.Id },
            UrunCevabiOlustur(urun, tumDepolar, new List<StokHareketi> { hareket }));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Yonetici")]
    public async Task<ActionResult<UrunCevabi>> Guncelle(int id, UrunGuncelleIstegi istek)
    {
        var urun = await _db.Urunler.FindAsync(id);
        if (urun == null)
            return NotFound(new { mesaj = "Ürün bulunamadı." });

        var kategori = await _db.Kategoriler.FindAsync(istek.KategoriId);
        if (kategori == null)
            return BadRequest(new { mesaj = "Seçilen kategori bulunamadı." });

        urun.Ad = istek.Ad.Trim();
        urun.Aciklama = string.IsNullOrWhiteSpace(istek.Aciklama) ? null : istek.Aciklama.Trim();
        urun.KategoriId = istek.KategoriId;
        urun.KritikStokSeviyesi = istek.KritikStokSeviyesi;

        await _db.SaveChangesAsync();

        if (istek.FotografiKaldir)
        {
            FotografSil(id);
        }
        else if (!string.IsNullOrWhiteSpace(istek.FotografBase64))
        {
            await FotografKaydetAsync(id, istek.FotografBase64);
        }

        var tumDepolar = await _db.Depolar.AsNoTracking().OrderBy(d => d.Ad).ToListAsync();
        var hareketler = await _db.StokHareketleri
            .AsNoTracking()
            .Where(h => h.UrunId == id)
            .ToListAsync();

        urun.Kategori = kategori;
        return Ok(UrunCevabiOlustur(urun, tumDepolar, hareketler));
    }

    private async Task<string> SonrakiUrunKodunuUretAsync()
    {
        var mevcutKodlar = await _db.Urunler
            .AsNoTracking()
            .Select(u => u.UrunKodu)
            .ToListAsync();

        long enBuyuk = BarkodBaslangicDegeri;
        foreach (var kod in mevcutKodlar)
        {
            if (long.TryParse(kod?.Trim(), out var sayi) && sayi > enBuyuk)
            {
                enBuyuk = sayi;
            }
        }

        var aday = (enBuyuk + 1).ToString();
        while (mevcutKodlar.Contains(aday))
        {
            enBuyuk++;
            aday = (enBuyuk + 1).ToString();
        }
        return aday;
    }

    private UrunCevabi UrunCevabiOlustur(
        Urun u,
        List<Depo> tumDepolar,
        List<StokHareketi> hareketler)
    {
        var urunHareketleri = hareketler.Where(h => h.UrunId == u.Id).ToList();

        // Tüm depoları listele (stoğu 0 olsa bile) ve stok sayısına göre azalan sırala
        var depoStoklari = tumDepolar
            .Select(d =>
            {
                var miktar = urunHareketleri
                    .Where(h => h.DepoId == d.Id)
                    .Sum(x => x.HareketTuru == "Cikis" ? -x.Miktar : x.Miktar);

                return new UrunDepoStokDto
                {
                    DepoId = d.Id,
                    DepoAdi = d.Ad,
                    DepoKonum = d.Konum,
                    Miktar = Math.Max(0, miktar)
                };
            })
            .OrderByDescending(x => x.Miktar)
            .ThenBy(x => x.DepoAdi)
            .ToList();

        return new UrunCevabi
        {
            Id = u.Id,
            UrunKodu = u.UrunKodu,
            Ad = u.Ad,
            Aciklama = u.Aciklama,
            MevcutMiktar = u.MevcutMiktar,
            KritikStokSeviyesi = u.KritikStokSeviyesi,
            KategoriId = u.KategoriId,
            KategoriAdi = u.Kategori != null ? u.Kategori.Ad : "",
            KritikMi = u.MevcutMiktar <= u.KritikStokSeviyesi,
            FotografBase64 = FotografOku(u.Id),
            DepoStoklari = depoStoklari
        };
    }

    private string FotografDosyaYolu(int urunId) =>
        Path.Combine(_gorselKlasoru, $"{urunId}.b64");

    private async Task FotografKaydetAsync(int urunId, string base64Veri)
    {
        Directory.CreateDirectory(_gorselKlasoru);
        await System.IO.File.WriteAllTextAsync(FotografDosyaYolu(urunId), base64Veri.Trim());
    }

    private string? FotografOku(int urunId)
    {
        var yol = FotografDosyaYolu(urunId);
        if (!System.IO.File.Exists(yol)) return null;
        try
        {
            var icerik = System.IO.File.ReadAllText(yol).Trim();
            return string.IsNullOrEmpty(icerik) ? null : icerik;
        }
        catch
        {
            return null;
        }
    }

    private void FotografSil(int urunId)
    {
        var yol = FotografDosyaYolu(urunId);
        if (System.IO.File.Exists(yol))
        {
            System.IO.File.Delete(yol);
        }
    }
}
