using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;

namespace StokTakip.Pages.Urunler;

[Authorize(Roles = "Yonetici")]
public class DuzenleModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public DuzenleModel(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [BindProperty]
    public Urun Urun { get; set; } = new();

    [BindProperty]
    public string? FotografBase64 { get; set; }

    [BindProperty]
    public bool FotografiKaldir { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var urun = await _db.Urunler.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (urun == null) return NotFound();
        Urun = urun;

        ViewData["DuzenlemeModu"] = true;
        ViewData["AtanacakBarkod"] = urun.UrunKodu;
        ViewData["MevcutFotografBase64"] = UrunFormYardimcisi.FotografOku(_env, id);

        await UrunFormYardimcisi.KategorileriYukleAsync(_db, ViewData, Urun.KategoriId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var kayit = await _db.Urunler.FindAsync(id);
        if (kayit == null) return NotFound();

        Urun.Id = id;
        Urun.UrunKodu = kayit.UrunKodu;
        Urun.MevcutMiktar = kayit.MevcutMiktar;
        ModelState.Remove("Urun.UrunKodu");

        if (ModelState.IsValid)
        {
            UrunFormYardimcisi.Normallestir(Urun);
            await UrunFormYardimcisi.EkDogrulamaAsync(_db, ModelState, Urun, haricId: id);
        }
        if (!ModelState.IsValid)
        {
            ViewData["DuzenlemeModu"] = true;
            ViewData["AtanacakBarkod"] = kayit.UrunKodu;
            ViewData["MevcutFotografBase64"] = FotografiKaldir
                ? null
                : (!string.IsNullOrWhiteSpace(FotografBase64) ? FotografBase64 : UrunFormYardimcisi.FotografOku(_env, id));
            await UrunFormYardimcisi.KategorileriYukleAsync(_db, ViewData, Urun.KategoriId);
            return Page();
        }

        kayit.Ad = Urun.Ad;
        kayit.Aciklama = Urun.Aciklama;
        kayit.KategoriId = Urun.KategoriId;
        kayit.KritikStokSeviyesi = Urun.KritikStokSeviyesi;
        await _db.SaveChangesAsync();

        if (FotografiKaldir)
        {
            UrunFormYardimcisi.FotografSil(_env, id);
        }
        else if (!string.IsNullOrWhiteSpace(FotografBase64))
        {
            await UrunFormYardimcisi.FotografKaydetAsync(_env, id, FotografBase64);
        }

        TempData["Basari"] = $"\"{kayit.Ad}\" ürünü güncellendi.";
        return RedirectToPage("Detay", new { id = kayit.Id });
    }

    public async Task<IActionResult> OnPostHizliKategoriEkleAsync([FromForm] string ad, [FromForm] string? aciklama)
    {
        var temizAd = (ad ?? string.Empty).Trim();
        if (temizAd.Length < 2)
            return BadRequest(new { mesaj = "Kategori adı en az 2 karakter olmalıdır." });

        if (await _db.Kategoriler.AnyAsync(k => k.Ad.ToLower() == temizAd.ToLower()))
            return new ConflictObjectResult(new { mesaj = "Bu isimde bir kategori zaten var." });

        var kategori = new Kategori
        {
            Ad = temizAd,
            Aciklama = string.IsNullOrWhiteSpace(aciklama) ? null : aciklama.Trim()
        };
        _db.Kategoriler.Add(kategori);
        await _db.SaveChangesAsync();

        return new JsonResult(new { id = kategori.Id, ad = kategori.Ad });
    }
}
