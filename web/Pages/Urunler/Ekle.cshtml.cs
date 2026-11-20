using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;

namespace StokTakip.Pages.Urunler;

[Authorize(Roles = "Yonetici")]
public class EkleModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<Kullanici> _userManager;
    private readonly IWebHostEnvironment _env;

    public EkleModel(AppDbContext db, UserManager<Kullanici> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [BindProperty]
    public Urun Urun { get; set; } = new()
    {
        KritikStokSeviyesi = 5,
        MevcutMiktar = 10
    };

    [BindProperty]
    public int DepoId { get; set; }

    [BindProperty]
    public string? FotografBase64 { get; set; }

    public async Task OnGetAsync(int? kategoriId, int? depoId)
    {
        if (kategoriId.HasValue) Urun.KategoriId = kategoriId.Value;
        if (depoId.HasValue) DepoId = depoId.Value;

        var otomatikKod = await UrunFormYardimcisi.SonrakiUrunKodunuUretAsync(_db);
        Urun.UrunKodu = otomatikKod;
        ViewData["AtanacakBarkod"] = otomatikKod;
        ViewData["DuzenlemeModu"] = false;

        await UrunFormYardimcisi.KategorileriYukleAsync(_db, ViewData, Urun.KategoriId);
        await UrunFormYardimcisi.DepolariYukleAsync(_db, ViewData, DepoId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Ürün kodu her zaman otomatik EAN-13 barkod olarak atanır
        Urun.UrunKodu = await UrunFormYardimcisi.SonrakiUrunKodunuUretAsync(_db);
        ModelState.Remove("Urun.UrunKodu");

        var depo = await _db.Depolar.FindAsync(DepoId);
        if (depo == null)
        {
            ModelState.AddModelError("DepoId", "Lütfen ürünün zimmetleneceği depoyu seçin.");
        }

        if (ModelState.IsValid)
        {
            UrunFormYardimcisi.Normallestir(Urun);
            await UrunFormYardimcisi.EkDogrulamaAsync(_db, ModelState, Urun);
        }

        if (!ModelState.IsValid)
        {
            ViewData["AtanacakBarkod"] = Urun.UrunKodu;
            ViewData["DuzenlemeModu"] = false;
            ViewData["MevcutFotografBase64"] = FotografBase64;
            await UrunFormYardimcisi.KategorileriYukleAsync(_db, ViewData, Urun.KategoriId);
            await UrunFormYardimcisi.DepolariYukleAsync(_db, ViewData, DepoId);
            return Page();
        }

        _db.Urunler.Add(Urun);
        await _db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(FotografBase64))
        {
            await UrunFormYardimcisi.FotografKaydetAsync(_env, Urun.Id, FotografBase64);
        }

        // Başlangıç stoğunu seçilen depoya zimmetle
        var hareket = new StokHareketi
        {
            UrunId = Urun.Id,
            DepoId = depo!.Id,
            HareketTuru = "Giris",
            Miktar = Urun.MevcutMiktar,
            Tarih = DateTime.UtcNow,
            KullaniciId = _userManager.GetUserId(User)
        };
        _db.StokHareketleri.Add(hareket);
        await _db.SaveChangesAsync();

        TempData["Basari"] = $"\"{Urun.Ad}\" ({Urun.UrunKodu}) eklendi ve {Urun.MevcutMiktar} adet başlangıç stoğu \"{depo.Ad}\" deposuna zimmetlendi.";
        return RedirectToPage("Detay", new { id = Urun.Id });
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

    public async Task<IActionResult> OnPostHizliDepoEkleAsync([FromForm] string ad, [FromForm] string? konum)
    {
        var temizAd = (ad ?? string.Empty).Trim();
        if (temizAd.Length < 2)
            return BadRequest(new { mesaj = "Depo adı en az 2 karakter olmalıdır." });

        if (await _db.Depolar.AnyAsync(d => d.Ad.ToLower() == temizAd.ToLower()))
            return new ConflictObjectResult(new { mesaj = "Bu isimde bir depo zaten kayıtlı." });

        var depo = new Depo
        {
            Ad = temizAd,
            Konum = string.IsNullOrWhiteSpace(konum) ? null : konum.Trim()
        };
        _db.Depolar.Add(depo);
        await _db.SaveChangesAsync();

        var gorunen = string.IsNullOrWhiteSpace(depo.Konum) ? depo.Ad : $"{depo.Ad} ({depo.Konum})";
        return new JsonResult(new { id = depo.Id, ad = gorunen });
    }
}
