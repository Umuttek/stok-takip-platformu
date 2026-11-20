using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;

namespace StokTakip.Pages.Urunler;

/// <summary>Ekle, Düzenle ve Detay sayfalarının ortak yardımcıları (Mobil/API ile birebir uyumlu).</summary>
internal static class UrunFormYardimcisi
{
    public const long BarkodBaslangicDegeri = 8690000000000L;

    public static async Task KategorileriYukleAsync(AppDbContext db, ViewDataDictionary viewData, int seciliId)
    {
        var kategoriler = await db.Kategoriler.AsNoTracking().OrderBy(k => k.Ad).ToListAsync();
        if (seciliId <= 0 && kategoriler.Count > 0)
            seciliId = kategoriler[0].Id;

        viewData["KategoriListesi"] = new SelectList(kategoriler, nameof(Kategori.Id), nameof(Kategori.Ad), seciliId);
        viewData["KategoriVar"] = kategoriler.Count > 0;
    }

    public static async Task DepolariYukleAsync(AppDbContext db, ViewDataDictionary viewData, int? seciliDepoId)
    {
        var depolar = await db.Depolar.AsNoTracking().OrderBy(d => d.Ad).ToListAsync();
        if ((!seciliDepoId.HasValue || seciliDepoId <= 0) && depolar.Count > 0)
            seciliDepoId = depolar[0].Id;

        viewData["DepoListesi"] = new SelectList(
            depolar.Select(d => new
            {
                d.Id,
                Ad = string.IsNullOrWhiteSpace(d.Konum) ? d.Ad : $"{d.Ad} ({d.Konum})"
            }),
            "Id",
            "Ad",
            seciliDepoId);
        viewData["DepoVar"] = depolar.Count > 0;
    }

    /// <summary>EAN-13 (8690000000001+) standardında bir sonraki otomatik barkod/ürün kodunu üretir.</summary>
    public static async Task<string> SonrakiUrunKodunuUretAsync(AppDbContext db)
    {
        var mevcutKodlar = await db.Urunler
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

    public static void Normallestir(Urun urun)
    {
        urun.UrunKodu = (urun.UrunKodu ?? string.Empty).Trim().ToUpperInvariant();
        urun.Ad = (urun.Ad ?? string.Empty).Trim();
        urun.Aciklama = string.IsNullOrWhiteSpace(urun.Aciklama) ? null : urun.Aciklama.Trim();
    }

    public static async Task EkDogrulamaAsync(AppDbContext db, ModelStateDictionary modelState, Urun urun, int? haricId = null)
    {
        if (!string.IsNullOrEmpty(urun.UrunKodu) &&
            await db.Urunler.AnyAsync(u => u.UrunKodu == urun.UrunKodu && u.Id != (haricId ?? 0)))
        {
            modelState.AddModelError("Urun.UrunKodu", "Bu ürün barkod kodu başka bir üründe kullanılıyor.");
        }

        if (urun.KategoriId <= 0 || !await db.Kategoriler.AnyAsync(k => k.Id == urun.KategoriId))
        {
            modelState.AddModelError("Urun.KategoriId", "Lütfen geçerli bir kategori seçin.");
        }
    }

    /// <summary>Web ve API/Mobil arasında ortak kullanılan UrunGorselleri klasör yolunu döndürür.</summary>
    public static string GorselKlasorYolu(IWebHostEnvironment env)
    {
        var apiGorselKlasoru = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "api", "UrunGorselleri"));
        var apiKlasoru = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "api"));
        if (Directory.Exists(apiKlasoru))
            return apiGorselKlasoru;

        return Path.Combine(env.ContentRootPath, "UrunGorselleri");
    }

    public static string? FotografOku(IWebHostEnvironment env, int urunId)
    {
        var yol = Path.Combine(GorselKlasorYolu(env), $"{urunId}.b64");
        if (!File.Exists(yol)) return null;
        try
        {
            var icerik = File.ReadAllText(yol).Trim();
            return string.IsNullOrEmpty(icerik) ? null : icerik;
        }
        catch
        {
            return null;
        }
    }

    public static async Task FotografKaydetAsync(IWebHostEnvironment env, int urunId, string base64Veri)
    {
        var klasor = GorselKlasorYolu(env);
        Directory.CreateDirectory(klasor);
        var temizBase64 = TemizBase64Ayikla(base64Veri);
        if (!string.IsNullOrEmpty(temizBase64))
        {
            await File.WriteAllTextAsync(Path.Combine(klasor, $"{urunId}.b64"), temizBase64);
        }
    }

    public static void FotografSil(IWebHostEnvironment env, int urunId)
    {
        var yol = Path.Combine(GorselKlasorYolu(env), $"{urunId}.b64");
        if (File.Exists(yol))
        {
            File.Delete(yol);
        }
    }

    private static string TemizBase64Ayikla(string girdi)
    {
        var veri = girdi.Trim();
        var virgulIndeks = veri.IndexOf(',');
        if (veri.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && virgulIndeks >= 0)
        {
            veri = veri[(virgulIndeks + 1)..].Trim();
        }
        return veri;
    }
}
