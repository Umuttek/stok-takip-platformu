using System.ComponentModel.DataAnnotations;
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
    public class HareketGirdi
    {
        [Required(ErrorMessage = "Ürün seçiniz.")]
        [Display(Name = "Ürün")]
        public int? UrunId { get; set; }
        [Required(ErrorMessage = "Depo seçiniz.")]
        [Display(Name = "Depo")]
        public int? DepoId { get; set; }
        [Required]
        [Display(Name = "Hareket Türü")]
        public string HareketTuru { get; set; } = "Giris";
        [Required(ErrorMessage = "Miktar giriniz.")]
        [Range(1, 100000, ErrorMessage = "Miktar 1 ile 100000 arasında olmalıdır.")]
        public int? Miktar { get; set; }
    }
    public async Task OnGetAsync(string? tur)
    {
        if (tur == "Cikis")
            Girdi.HareketTuru = "Cikis";
        await ListeleriDoldurAsync();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (Girdi.HareketTuru != "Giris" && Girdi.HareketTuru != "Cikis")
            ModelState.AddModelError("Girdi.HareketTuru", "Geçersiz hareket türü.");
        if (!ModelState.IsValid)
        {
            await ListeleriDoldurAsync();
            return Page();
        }
        var urun = await _context.Urunler.FindAsync(Girdi.UrunId!.Value);
        var depoVar = await _context.Depolar.AnyAsync(d => d.Id == Girdi.DepoId);
        if (urun == null || !depoVar)
        {
            ModelState.AddModelError(string.Empty, "Seçilen ürün veya depo bulunamadı.");
            await ListeleriDoldurAsync();
            return Page();
        }
        int miktar = Girdi.Miktar!.Value;
        if (Girdi.HareketTuru == "Cikis")
        {
            if (urun.MevcutMiktar < miktar)
            {
                ModelState.AddModelError("Girdi.Miktar", $"Yetersiz stok. Mevcut miktar: {urun.MevcutMiktar}");
                await ListeleriDoldurAsync();
                return Page();
            }
            urun.MevcutMiktar -= miktar;
        }
        else
        {
            urun.MevcutMiktar += miktar;
        }
        var hareket = new StokHareketi
        {
            UrunId = urun.Id,
            DepoId = Girdi.DepoId!.Value,
            HareketTuru = Girdi.HareketTuru,
            Miktar = miktar,
            Tarih = DateTime.UtcNow,
            KullaniciId = _userManager.GetUserId(User)
        };
        _context.StokHareketleri.Add(hareket);
        await _context.SaveChangesAsync();
        string islem = Girdi.HareketTuru == "Giris" ? "girişi" : "çıkışı";
        TempData["Mesaj"] = $"{urun.Ad} için stok {islem} kaydedildi. Güncel miktar: {urun.MevcutMiktar}";
        if (urun.MevcutMiktar <= urun.KritikStokSeviyesi)
            TempData["Uyari"] = $"Dikkat: {urun.Ad} kritik stok seviyesinde ({urun.MevcutMiktar}, kritik seviye: {urun.KritikStokSeviyesi}).";

        return RedirectToPage("./Index");
    }
    private async Task ListeleriDoldurAsync()
    {
        var urunler = await _context.Urunler.OrderBy(u => u.Ad).ToListAsync();
        var depolar = await _context.Depolar.OrderBy(d => d.Ad).ToListAsync();
        UrunListesi = new SelectList(
            urunler.Select(u => new { u.Id, Metin = $"{u.Ad} (Stok: {u.MevcutMiktar})" }),
            "Id", "Metin");
        DepoListesi = new SelectList(depolar, "Id", "Ad");
    }
}