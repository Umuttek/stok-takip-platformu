using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;

namespace StokTakip.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<Kullanici> _userManager;

    public IndexModel(AppDbContext db, UserManager<Kullanici> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public string? KullaniciAdSoyad { get; set; }
    public int UrunCesidiSayisi { get; set; }
    public long ToplamUrunAdeti { get; set; }
    public int DepoSayisi { get; set; }
    public int KategoriSayisi { get; set; }
    public int KritikStokUrunSayisi { get; set; }
    public List<Urun> KritikUrunler { get; set; } = new();

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var kullanici = await _userManager.GetUserAsync(User);
            KullaniciAdSoyad = !string.IsNullOrWhiteSpace(kullanici?.AdSoyad)
                ? kullanici.AdSoyad
                : User.Identity.Name;
        }

        UrunCesidiSayisi = await _db.Urunler.CountAsync();
        ToplamUrunAdeti = await _db.Urunler.SumAsync(u => (long?)u.MevcutMiktar) ?? 0;
        DepoSayisi = await _db.Depolar.CountAsync();
        KategoriSayisi = await _db.Kategoriler.CountAsync();
        KritikStokUrunSayisi = await _db.Urunler
            .CountAsync(u => u.MevcutMiktar <= u.KritikStokSeviyesi);

        KritikUrunler = await _db.Urunler
            .AsNoTracking()
            .Include(u => u.Kategori)
            .Where(u => u.MevcutMiktar <= u.KritikStokSeviyesi)
            .OrderBy(u => u.MevcutMiktar)
            .Take(5)
            .ToListAsync();
    }
}
