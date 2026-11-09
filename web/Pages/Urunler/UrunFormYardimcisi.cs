using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;
namespace StokTakip.Pages.Urunler;

/// <summary>Ekle ve Düzenle sayfalarının ortak yardımcıları.</summary>
internal static class UrunFormYardimcisi
{
    public static async Task KategorileriYukleAsync(AppDbContext db, ViewDataDictionary viewData, int seciliId)
    {
        var kategoriler = await db.Kategoriler.AsNoTracking().OrderBy(k => k.Ad).ToListAsync();
        viewData["KategoriListesi"] = new SelectList(kategoriler, nameof(Kategori.Id), nameof(Kategori.Ad), seciliId);
        viewData["KategoriVar"] = kategoriler.Count > 0;
    }

    /// <summary>Kodu karşılaştırmayı kolaylaştırmak için büyük harfe çevirir, boşlukları temizler.</summary>
    public static void Normallestir(Urun urun)
    {
        urun.UrunKodu = (urun.UrunKodu ?? string.Empty).Trim().ToUpperInvariant();
        urun.Ad = (urun.Ad ?? string.Empty).Trim();
        urun.Aciklama = string.IsNullOrWhiteSpace(urun.Aciklama) ? null : urun.Aciklama.Trim();
    }

    /// <summary>Model kurallarının ötesindeki, veritabanı gerektiren doğrulamalar.</summary>
    public static async Task EkDogrulamaAsync(AppDbContext db, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState, Urun urun, int? haricId = null)
    {
        if (await db.Urunler.AnyAsync(u => u.UrunKodu == urun.UrunKodu && u.Id != (haricId ?? 0)))
            modelState.AddModelError("Urun.UrunKodu", "Bu ürün kodu başka bir üründe kullanılıyor.");
        if (urun.KategoriId > 0 && !await db.Kategoriler.AnyAsync(k => k.Id == urun.KategoriId))
            modelState.AddModelError("Urun.KategoriId", "Seçilen kategori bulunamadı.");
    }
}

