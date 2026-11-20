using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;

namespace StokTakip.Pages.Yonetim;

[Authorize(Roles = "Yonetici")]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<Kullanici> _userManager;

    public IndexModel(AppDbContext db, UserManager<Kullanici> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // Genel Özet İstatistikleri
    public int UrunCesidiSayisi { get; set; }
    public long ToplamStokAdedi { get; set; }
    public int KritikUrunSayisi { get; set; }
    public int KategoriSayisi { get; set; }
    public int DepoSayisi { get; set; }
    public int ToplamHareketSayisi { get; set; }
    public int GirisHareketSayisi { get; set; }
    public int CikisHareketSayisi { get; set; }
    public long ToplamGirenBirim { get; set; }
    public long ToplamCikanBirim { get; set; }
    public int Son7GunHareketSayisi { get; set; }
    public int ToplamKullaniciSayisi { get; set; }
    public int YoneticiSayisi { get; set; }
    public int CalisanSayisi { get; set; }

    // Tablo ve Özet Listeleri
    public List<Urun> KritikUrunler { get; set; } = new();
    public List<StokHareketi> SonHareketler { get; set; } = new();
    public List<DepoOzetSatiri> DepoOzetleri { get; set; } = new();
    public List<KategoriOzetSatiri> KategoriOzetleri { get; set; } = new();
    public List<KullaniciOzetSatiri> Kullanicilar { get; set; } = new();

    [BindProperty]
    public YeniKullaniciGirdi YeniKullanici { get; set; } = new();

    public record DepoOzetSatiri(
        int Id,
        string Ad,
        string? Konum,
        int UrunCesidi,
        int ZimmetliAdet,
        int HareketSayisi);

    public record KategoriOzetSatiri(
        int Id,
        string Ad,
        int UrunCesidi,
        int ToplamStok,
        int KritikUrunSayisi);

    public record KullaniciOzetSatiri(
        string Id,
        string AdSoyad,
        string Email,
        string Rol,
        bool YoneticiMi);

    public class YeniKullaniciGirdi
    {
        [Required(ErrorMessage = "Ad soyad boş bırakılamaz.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad Soyad {2}-{1} karakter arasında olmalıdır.")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre boş bırakılamaz.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az {2} karakter olmalı.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı boş bırakılamaz.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Sifre), ErrorMessage = "Şifreler eşleşmiyor.")]
        [Display(Name = "Şifre (Tekrar)")]
        public string SifreTekrar { get; set; } = string.Empty;

        [Display(Name = "Yönetici yetkisi ver")]
        public bool YoneticiYetkisi { get; set; }
    }

    public async Task OnGetAsync()
    {
        await VerileriYukleAsync();
    }

    public async Task<IActionResult> OnPostKullaniciEkleAsync()
    {
        if (!ModelState.IsValid)
        {
            await VerileriYukleAsync();
            return Page();
        }

        var email = YeniKullanici.Email.Trim();
        if (await _userManager.FindByEmailAsync(email) != null)
        {
            ModelState.AddModelError("YeniKullanici.Email", "Bu e-posta adresiyle kayıtlı bir kullanıcı zaten var.");
            await VerileriYukleAsync();
            return Page();
        }

        var kullanici = new Kullanici
        {
            UserName = email,
            Email = email,
            AdSoyad = YeniKullanici.AdSoyad.Trim(),
            EmailConfirmed = true
        };

        var sonuc = await _userManager.CreateAsync(kullanici, YeniKullanici.Sifre);
        if (!sonuc.Succeeded)
        {
            foreach (var err in sonuc.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            await VerileriYukleAsync();
            return Page();
        }

        var atanacakRol = YeniKullanici.YoneticiYetkisi ? "Yonetici" : "Calisan";
        await _userManager.AddToRoleAsync(kullanici, atanacakRol);

        var rolEtiketi = YeniKullanici.YoneticiYetkisi ? "Yönetici" : "Çalışan";
        TempData["Basari"] = $"\"{kullanici.AdSoyad}\" ({email}) kullanıcısı \"{rolEtiketi}\" yetkisiyle eklendi.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRolDegistirAsync(string kullaniciId, string yeniRol)
    {
        if (yeniRol != "Yonetici" && yeniRol != "Calisan")
        {
            TempData["Hata"] = "Geçersiz rol seçimi.";
            return RedirectToPage();
        }

        var mevcutKullaniciId = _userManager.GetUserId(User);
        if (kullaniciId == mevcutKullaniciId && yeniRol != "Yonetici")
        {
            TempData["Hata"] = "Kendi yönetici yetkinizi kaldıramazsınız.";
            return RedirectToPage();
        }

        var hedef = await _userManager.FindByIdAsync(kullaniciId);
        if (hedef == null)
        {
            TempData["Hata"] = "Kullanıcı bulunamadı.";
            return RedirectToPage();
        }

        var mevcutRoller = await _userManager.GetRolesAsync(hedef);
        if (mevcutRoller.Count > 0)
            await _userManager.RemoveFromRolesAsync(hedef, mevcutRoller);

        await _userManager.AddToRoleAsync(hedef, yeniRol);
        TempData["Basari"] = $"\"{hedef.AdSoyad}\" kullanıcısının yetkisi \"{(yeniRol == "Yonetici" ? "Yönetici" : "Çalışan")}\" olarak güncellendi.";
        return RedirectToPage();
    }

    private async Task VerileriYukleAsync()
    {
        UrunCesidiSayisi = await _db.Urunler.CountAsync();
        ToplamStokAdedi = await _db.Urunler.SumAsync(u => (long?)u.MevcutMiktar) ?? 0;
        KritikUrunSayisi = await _db.Urunler.CountAsync(u => u.MevcutMiktar <= u.KritikStokSeviyesi);
        KategoriSayisi = await _db.Kategoriler.CountAsync();
        DepoSayisi = await _db.Depolar.CountAsync();

        ToplamHareketSayisi = await _db.StokHareketleri.CountAsync();
        GirisHareketSayisi = await _db.StokHareketleri.CountAsync(h => h.HareketTuru == "Giris");
        CikisHareketSayisi = await _db.StokHareketleri.CountAsync(h => h.HareketTuru == "Cikis");
        ToplamGirenBirim = await _db.StokHareketleri
            .Where(h => h.HareketTuru == "Giris")
            .SumAsync(h => (long?)h.Miktar) ?? 0;
        ToplamCikanBirim = await _db.StokHareketleri
            .Where(h => h.HareketTuru == "Cikis")
            .SumAsync(h => (long?)h.Miktar) ?? 0;

        var yediGunOnce = DateTime.UtcNow.AddDays(-7);
        Son7GunHareketSayisi = await _db.StokHareketleri.CountAsync(h => h.Tarih >= yediGunOnce);

        KritikUrunler = await _db.Urunler
            .AsNoTracking()
            .Include(u => u.Kategori)
            .Where(u => u.MevcutMiktar <= u.KritikStokSeviyesi)
            .OrderBy(u => u.MevcutMiktar)
            .Take(8)
            .ToListAsync();

        SonHareketler = await _db.StokHareketleri
            .AsNoTracking()
            .Include(h => h.Urun)
            .Include(h => h.Depo)
            .Include(h => h.Kullanici)
            .OrderByDescending(h => h.Tarih)
            .ThenByDescending(h => h.Id)
            .Take(8)
            .ToListAsync();

        var depolar = await _db.Depolar.AsNoTracking().OrderBy(d => d.Ad).ToListAsync();
        var tumHareketler = await _db.StokHareketleri.AsNoTracking().ToListAsync();

        DepoOzetleri = depolar.Select(d =>
        {
            var dHareket = tumHareketler.Where(h => h.DepoId == d.Id).ToList();
            var zimmetliUrunler = dHareket
                .GroupBy(h => h.UrunId)
                .Select(g => g.Sum(x => x.HareketTuru == "Cikis" ? -x.Miktar : x.Miktar))
                .Where(m => m > 0)
                .ToList();

            return new DepoOzetSatiri(
                d.Id,
                d.Ad,
                d.Konum,
                zimmetliUrunler.Count,
                zimmetliUrunler.Sum(),
                dHareket.Count);
        }).ToList();

        KategoriOzetleri = await _db.Kategoriler
            .AsNoTracking()
            .OrderBy(k => k.Ad)
            .Select(k => new KategoriOzetSatiri(
                k.Id,
                k.Ad,
                k.Urunler.Count,
                k.Urunler.Sum(u => (int?)u.MevcutMiktar) ?? 0,
                k.Urunler.Count(u => u.MevcutMiktar <= u.KritikStokSeviyesi)))
            .ToListAsync();

        var tumKullanicilar = await _userManager.Users
            .AsNoTracking()
            .OrderBy(k => k.AdSoyad)
            .ToListAsync();

        var kullaniciListesi = new List<KullaniciOzetSatiri>();
        foreach (var k in tumKullanicilar)
        {
            var roller = await _userManager.GetRolesAsync(k);
            var yoneticiMi = roller.Contains("Yonetici");
            kullaniciListesi.Add(new KullaniciOzetSatiri(
                k.Id,
                string.IsNullOrWhiteSpace(k.AdSoyad) ? (k.Email ?? "-") : k.AdSoyad,
                k.Email ?? "-",
                yoneticiMi ? "Yönetici" : "Çalışan",
                yoneticiMi));
        }

        Kullanicilar = kullaniciListesi;
        ToplamKullaniciSayisi = Kullanicilar.Count;
        YoneticiSayisi = Kullanicilar.Count(k => k.YoneticiMi);
        CalisanSayisi = Kullanicilar.Count(k => !k.YoneticiMi);
    }
}
