using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;

namespace StokTakip.Pages.StokHareketleri;

[Authorize]
public class EkleModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<Kullanici> _userManager;

    public EkleModel(AppDbContext context, UserManager<Kullanici> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public HareketGirdi Girdi { get; set; } = new();

    public SelectList UrunListesi { get; set; } = default!;
    public SelectList DepoListesi { get; set; } = default!;
    public bool UrunVar { get; set; }
    public bool DepoVar { get; set; }
    public string UrunStokJson { get; set; } = "[]";
    public List<StokHareketi> SonHareketler { get; set; } = new();

    public class HareketGirdi
    {
        [Required(ErrorMessage = "Lütfen bir ürün seçiniz.")]
        [Display(Name = "Ürün")]
        public int? UrunId { get; set; }

        [Required(ErrorMessage = "Lütfen işlem yapılacak depoyu seçiniz.")]
        [Display(Name = "İşlem Yapılacak Depo")]
        public int? DepoId { get; set; }

        [Required(ErrorMessage = "Hareket türü seçiniz.")]
        [Display(Name = "Hareket Türü")]
        public string HareketTuru { get; set; } = "Giris";

        [Required(ErrorMessage = "Lütfen miktar giriniz.")]
        [Range(1, 100000, ErrorMessage = "Miktar 1 ile 100.000 arasında olmalıdır.")]
        [Display(Name = "Miktar (Adet)")]
        public int? Miktar { get; set; } = 1;
    }

    public async Task OnGetAsync(string? tur, int? urunId, int? depoId)
    {
        Girdi.HareketTuru = string.Equals(tur, "Cikis", StringComparison.OrdinalIgnoreCase) ? "Cikis" : "Giris";
        if (urunId.HasValue)
            Girdi.UrunId = urunId.Value;
        if (depoId.HasValue)
            Girdi.DepoId = depoId.Value;

        await ListeleriDoldurAsync();
    }

    public async Task<IActionResult> OnPostAsync(string? devam)
    {
        if (Girdi.HareketTuru != "Giris" && Girdi.HareketTuru != "Cikis")
            ModelState.AddModelError("Girdi.HareketTuru", "Geçersiz hareket türü. Giriş veya Çıkış seçiniz.");

        if (!ModelState.IsValid)
        {
            await ListeleriDoldurAsync();
            return Page();
        }

        var urun = await _context.Urunler.FindAsync(Girdi.UrunId!.Value);
        var depo = await _context.Depolar.FindAsync(Girdi.DepoId!.Value);
        if (urun == null || depo == null)
        {
            ModelState.AddModelError(string.Empty, "Seçilen ürün veya depo bulunamadı.");
            await ListeleriDoldurAsync();
            return Page();
        }

        int miktar = Girdi.Miktar!.Value;

        // Depodaki mevcut miktarı hesapla (API/Mobil ile birebir aynı kontrol)
        var depoHareketleri = await _context.StokHareketleri
            .AsNoTracking()
            .Where(h => h.UrunId == urun.Id && h.DepoId == depo.Id)
            .ToListAsync();

        int depodakiMiktar = Math.Max(0, depoHareketleri.Sum(h => h.HareketTuru == "Cikis" ? -h.Miktar : h.Miktar));

        if (Girdi.HareketTuru == "Cikis")
        {
            if (depodakiMiktar < miktar)
            {
                ModelState.AddModelError("Girdi.Miktar",
                    $"\"{depo.Ad}\" deposunda yeterli \"{urun.Ad}\" stoğu yok. Bu depodaki mevcut miktar: {depodakiMiktar} adet.");
                await ListeleriDoldurAsync();
                return Page();
            }

            urun.MevcutMiktar = Math.Max(0, urun.MevcutMiktar - miktar);
        }
        else
        {
            urun.MevcutMiktar += miktar;
        }

        var hareket = new StokHareketi
        {
            UrunId = urun.Id,
            DepoId = depo.Id,
            HareketTuru = Girdi.HareketTuru,
            Miktar = miktar,
            Tarih = DateTime.UtcNow,
            KullaniciId = _userManager.GetUserId(User)
        };

        _context.StokHareketleri.Add(hareket);
        await _context.SaveChangesAsync();

        string islem = Girdi.HareketTuru == "Giris" ? "stok girişi (+)" : "stok çıkışı (-)";
        TempData["Basari"] = $"\"{urun.Ad}\" ürünü için \"{depo.Ad}\" deposuna {miktar} adet {islem} kaydedildi. Güncel toplam stok: {urun.MevcutMiktar}.";

        if (urun.MevcutMiktar <= urun.KritikStokSeviyesi)
        {
            TempData["Uyari"] = $"Kritik Stok Uyarısı: \"{urun.Ad}\" kritik stok seviyesinde veya altında! (Mevcut: {urun.MevcutMiktar}, Kritik Seviye: {urun.KritikStokSeviyesi})";
        }

        if (string.Equals(devam, "yeni", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage("./Ekle", new
            {
                tur = Girdi.HareketTuru,
                depoId = Girdi.DepoId,
                urunId = Girdi.UrunId
            });
        }

        return RedirectToPage("./Index");
    }

    private async Task ListeleriDoldurAsync()
    {
        var urunler = await _context.Urunler
            .AsNoTracking()
            .Include(u => u.Kategori)
            .OrderBy(u => u.Ad)
            .ToListAsync();

        var depolar = await _context.Depolar
            .AsNoTracking()
            .OrderBy(d => d.Ad)
            .ToListAsync();

        UrunVar = urunler.Count > 0;
        DepoVar = depolar.Count > 0;

        if (!Girdi.DepoId.HasValue && depolar.Count > 0)
            Girdi.DepoId = depolar[0].Id;

        if (!Girdi.UrunId.HasValue && urunler.Count > 0)
            Girdi.UrunId = urunler[0].Id;

        UrunListesi = new SelectList(
            urunler.Select(u => new
            {
                u.Id,
                Metin = $"{u.Ad} ({u.UrunKodu})"
            }),
            "Id",
            "Metin",
            Girdi.UrunId);

        DepoListesi = new SelectList(
            depolar.Select(d => new
            {
                d.Id,
                Metin = string.IsNullOrWhiteSpace(d.Konum) ? d.Ad : $"{d.Ad} ({d.Konum})"
            }),
            "Id",
            "Metin",
            Girdi.DepoId);

        var depoStokGruplari = await _context.StokHareketleri
            .AsNoTracking()
            .GroupBy(h => new { h.UrunId, h.DepoId })
            .Select(g => new
            {
                g.Key.UrunId,
                g.Key.DepoId,
                NetMiktar = g.Sum(h => h.HareketTuru == "Cikis" ? -h.Miktar : h.Miktar)
            })
            .ToListAsync();

        var urunDtoListesi = urunler.Select(u => new
        {
            id = u.Id,
            kod = u.UrunKodu,
            ad = u.Ad,
            kategori = u.Kategori?.Ad ?? "-",
            mevcutMiktar = u.MevcutMiktar,
            kritikSeviye = u.KritikStokSeviyesi,
            depoStoklari = depoStokGruplari
                .Where(x => x.UrunId == u.Id)
                .ToDictionary(x => x.DepoId.ToString(), x => Math.Max(0, x.NetMiktar))
        });

        UrunStokJson = JsonSerializer.Serialize(urunDtoListesi);

        SonHareketler = await _context.StokHareketleri
            .AsNoTracking()
            .Include(h => h.Urun)
            .Include(h => h.Depo)
            .Include(h => h.Kullanici)
            .OrderByDescending(h => h.Tarih)
            .Take(6)
            .ToListAsync();
    }
}
